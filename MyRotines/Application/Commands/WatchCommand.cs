using MyRotines.Application.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public sealed class WatchCommand(
    FileMonitorService monitorService,
    ExtractionService extractionService,
    FileMoveService moveService) : AsyncCommand<WatchCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--path <PATH>")]
        [Description("Diretório monitorado")]
        public string Path { get; set; } = Directory.GetCurrentDirectory();

        [CommandOption("--filter <PATTERN>")]
        [Description("Filtro do watcher (ex.: *.zip)")]
        public string Filter { get; set; } = "*.zip";

        [CommandOption("--recursive")]
        [Description("Monitora subpastas")]
        public bool Recursive { get; set; }

        [CommandOption("--extract")]
        [Description("Extrai arquivos zip quando detectados")]
        public bool Extract { get; set; }

        [CommandOption("--move-to <PATH>")]
        [Description("Move arquivo/pasta final para destino")]
        public string? MoveTo { get; set; }

        [CommandOption("--stable-seconds <N>")]
        [Description("Tempo de estabilidade do arquivo antes de processar")]
        public int StableSeconds { get; set; } = 2;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var rootPath = System.IO.Path.GetFullPath(settings.Path);

        if (!Directory.Exists(rootPath))
        {
            AnsiConsole.MarkupLine($"[red]Diretório não encontrado:[/] {rootPath}");
            return -1;
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            linkedCts.Cancel();
        };

        AnsiConsole.Write(new Panel($"Monitorando [yellow]{rootPath}[/] com filtro [yellow]{settings.Filter}[/]. Pressione Ctrl+C para sair.")
            .Header("[bold cyan]Watch Service[/]")
            .Expand());

        await monitorService.MonitorAsync(
            rootPath,
            settings.Filter,
            settings.Recursive,
            async (filePath, token) =>
            {
                await WaitUntilStableAsync(filePath, settings.StableSeconds, token);

                var finalPath = filePath;
                AnsiConsole.MarkupLine($"[blue]Detectado:[/] {filePath}");

                if (settings.Extract && filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    var destination = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(filePath) ?? rootPath,
                        System.IO.Path.GetFileNameWithoutExtension(filePath));

                    await extractionService.ExtractAsync(filePath, destination, token);
                    finalPath = destination;
                    AnsiConsole.MarkupLine($"[green]Extraído:[/] {destination}");
                }

                if (!string.IsNullOrWhiteSpace(settings.MoveTo))
                {
                    finalPath = await moveService.MoveAsync(finalPath, settings.MoveTo, overwrite: true, token);
                    AnsiConsole.MarkupLine($"[green]Movido para:[/] {finalPath}");
                }
            },
            linkedCts.Token);

        return 0;
    }

    private static async Task WaitUntilStableAsync(string filePath, int stableSeconds, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        long previousLength = -1;
        var stableFor = 0;

        while (stableFor < stableSeconds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(filePath))
            {
                return;
            }

            var currentLength = new FileInfo(filePath).Length;

            if (currentLength == previousLength)
            {
                stableFor++;
            }
            else
            {
                stableFor = 0;
                previousLength = currentLength;
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
