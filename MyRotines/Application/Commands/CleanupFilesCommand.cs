using MyRotines.Application.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public sealed class CleanupFilesCommand(FileCleanupService cleanupService) : Command<CleanupFilesCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--path <PATH>")]
        [Description("Diretório alvo")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        [CommandOption("--days <N>")]
        [Description("Remover arquivos mais antigos que N dias")]
        public int OlderThanDays { get; set; } = 30;

        [CommandOption("--pattern <PATTERN>")]
        [Description("Padrão de busca (ex.: *.zip)")]
        public string Pattern { get; set; } = "*.*";

        [CommandOption("--recursive")]
        [Description("Busca recursiva")]
        public bool Recursive { get; set; }

        [CommandOption("--dry-run")]
        [Description("Somente simula, sem apagar")]
        public bool DryRun { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var path = System.IO.Path.GetFullPath(settings.Path);
        var result = cleanupService.CleanupOldFiles(path, settings.OlderThanDays, settings.Pattern, settings.Recursive, settings.DryRun);

        var table = new Table().Border(TableBorder.Rounded).AddColumns("Métrica", "Valor");
        table.AddRow("Matched", result.TotalMatched.ToString());
        table.AddRow("Deleted", result.TotalDeleted.ToString());
        table.AddRow("Freed MB", (result.TotalFreedBytes / 1024d / 1024d).ToString("N2"));
        table.AddRow("Mode", settings.DryRun ? "DRY-RUN" : "EXECUTION");

        AnsiConsole.Write(new Panel(table).Header("[bold cyan]Cleanup de Arquivos[/]").Expand());
        return 0;
    }
}
