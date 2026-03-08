using MyRotines.Application.Routines;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;

namespace MyRotines.Application.Commands;

public class RoutineCommand(
    RoutineStepCatalog stepCatalog,
    RoutineProfilesOptions profilesOptions) : AsyncCommand<RoutineCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--url <URL>")]
        [Description("URL do arquivo")]
        public string Url { get; set; } = string.Empty;

        [CommandOption("--workdir <PATH>")]
        [Description("Diretório de trabalho (default: diretório atual)")]
        public string WorkDirectory { get; set; } = Directory.GetCurrentDirectory();

        [CommandOption("--name <FILE>")]
        [Description("Nome do arquivo final (default: nome inferido da URL)")]
        public string? FileName { get; set; }

        [CommandOption("--move-to <PATH>")]
        [Description("Diretório final para organizar o arquivo/pasta")]
        public string? MoveTo { get; set; }

        [CommandOption("--cleanup-downloaded")]
        [Description("Remove zip no step cleanup")]
        public bool CleanupDownloadedFile { get; set; }

        [CommandOption("--steps <CSV>")]
        [Description("Sequência customizada de steps. Ex: download,extract,move,restore-sql,cleanup")]
        public string? StepsCsv { get; set; }

        [CommandOption("--profile <NAME>")]
        [Description("Perfil de steps definido em appsettings.json")]
        public string? Profile { get; set; }

        [CommandOption("--move-extract")]
        [Description("Atalho: download,extract,move")]
        public bool MoveExtract { get; set; }

        [CommandOption("--sql-server <SERVER>")]
        [Description("Servidor SQL para step restore-sql")]
        public string? SqlServer { get; set; }

        [CommandOption("--sql-db <DB>")]
        [Description("Banco SQL destino para step restore-sql")]
        public string? SqlDatabase { get; set; }

        [CommandOption("--sql-trusted")]
        [Description("Autenticação integrada Windows")]
        public bool SqlTrusted { get; set; }

        [CommandOption("--sql-user <USER>")]
        [Description("Usuário SQL")]
        public string? SqlUser { get; set; }

        [CommandOption("--sql-pass <PASS>")]
        [Description("Senha SQL")]
        public string? SqlPassword { get; set; }

        [CommandOption("--sql-replace")]
        [Description("RESTORE com WITH REPLACE")]
        public bool SqlReplace { get; set; }

        [CommandOption("--sql-backup <PATH>")]
        [Description("Arquivo .bak explícito para restore-sql")]
        public string? SqlBackupFile { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            AnsiConsole.MarkupLine("[red]URL é obrigatória[/]");
            return -1;
        }

        var selectedSteps = ResolveSteps(settings);
        if (selectedSteps.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Nenhum step selecionado[/]");
            return -1;
        }

        if (selectedSteps.Contains("move", StringComparer.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(settings.MoveTo))
        {
            AnsiConsole.MarkupLine("[red]Step 'move' exige --move-to[/]");
            return -1;
        }

        if (selectedSteps.Contains("restore-sql", StringComparer.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(settings.SqlServer) || string.IsNullOrWhiteSpace(settings.SqlDatabase)))
        {
            AnsiConsole.MarkupLine("[red]Step 'restore-sql' exige --sql-server e --sql-db[/]");
            return -1;
        }

        var unknownSteps = selectedSteps.Where(stepKey => !stepCatalog.TryGet(stepKey, out _)).ToArray();
        if (unknownSteps.Length > 0)
        {
            AnsiConsole.MarkupLine($"[red]Steps inválidos:[/] {string.Join(", ", unknownSteps)}");
            return -1;
        }

        var routineContext = new RoutineExecutionContext(
            settings.Url,
            settings.WorkDirectory,
            settings.FileName,
            settings.MoveTo,
            settings.CleanupDownloadedFile,
            new SqlRestoreOptions
            {
                Server = settings.SqlServer,
                Database = settings.SqlDatabase,
                TrustedConnection = settings.SqlTrusted,
                User = settings.SqlUser,
                Password = settings.SqlPassword,
                ReplaceDatabase = settings.SqlReplace,
                BackupFilePath = settings.SqlBackupFile
            });

        RenderHeader(settings, selectedSteps, routineContext);

        var results = new List<(string Step, bool Success, TimeSpan Duration, string? Error)>();

        try
        {
            await AnsiConsole.Progress()
                .AutoClear(false)
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn(),
                    new ElapsedTimeColumn()
                ])
                .StartAsync(async progressContext =>
                {
                    foreach (var stepKey in selectedSteps)
                    {
                        _ = stepCatalog.TryGet(stepKey, out var step);
                        var task = progressContext.AddTask($"[yellow]{stepKey}[/]", maxValue: 100);
                        var watch = Stopwatch.StartNew();

                        try
                        {
                            await step!.ExecuteAsync(routineContext, cancellationToken);
                            watch.Stop();
                            task.Increment(100);
                            task.Description = $"[green]OK[/] {stepKey}";
                            results.Add((stepKey, true, watch.Elapsed, null));
                        }
                        catch (Exception ex)
                        {
                            watch.Stop();
                            task.Increment(100);
                            task.Description = $"[red]Erro[/] {stepKey}";
                            results.Add((stepKey, false, watch.Elapsed, ex.Message));
                            throw;
                        }
                    }
                });
        }
        catch
        {
            RenderResultTable(results);
            AnsiConsole.MarkupLine("[red]Rotina interrompida.[/]");
            return -1;
        }

        RenderResultTable(results);

        var finalPath = routineContext.FinalPath ?? routineContext.OutputPath;
        AnsiConsole.MarkupLine($"[green]Rotina finalizada com sucesso.[/] Resultado: {finalPath}");

        return 0;
    }

    private List<string> ResolveSteps(Settings settings)
    {
        if (settings.MoveExtract)
        {
            return ["download", "extract", "move"];
        }

        if (!string.IsNullOrWhiteSpace(settings.StepsCsv))
        {
            return settings.StepsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        var profile = string.IsNullOrWhiteSpace(settings.Profile) ? "default" : settings.Profile;

        if (profile is not null && profilesOptions.Profiles.TryGetValue(profile, out var profileSteps))
        {
            return [.. profileSteps];
        }

        return ["download", "extract"];
    }

    private static void RenderHeader(Settings settings, IReadOnlyCollection<string> steps, RoutineExecutionContext context)
    {
        var table = new Table().Border(TableBorder.Rounded).AddColumns("Configuração", "Valor");
        table.AddRow("Steps", string.Join(" -> ", steps));
        table.AddRow("Workdir", context.WorkDirectory);
        table.AddRow("Output", context.OutputPath);
        table.AddRow("MoveTo", settings.MoveTo ?? "(não definido)");
        table.AddRow("SQL", string.IsNullOrWhiteSpace(settings.SqlServer) ? "(não definido)" : $"{settings.SqlServer} / {settings.SqlDatabase}");
        table.AddRow("Cleanup", settings.CleanupDownloadedFile ? "sim" : "não");

        AnsiConsole.Write(new Panel(table)
            .Header("[bold cyan]Routine Pipeline[/]")
            .Border(BoxBorder.Rounded)
            .Expand());
    }

    private static void RenderResultTable(IReadOnlyCollection<(string Step, bool Success, TimeSpan Duration, string? Error)> results)
    {
        var table = new Table().Border(TableBorder.Rounded).AddColumns("Step", "Status", "Duração", "Detalhe");

        foreach (var result in results)
        {
            table.AddRow(
                result.Step,
                result.Success ? "[green]OK[/]" : "[red]Falha[/]",
                $"{result.Duration.TotalMilliseconds:0} ms",
                result.Error ?? "-");
        }

        AnsiConsole.Write(table);
    }
}
