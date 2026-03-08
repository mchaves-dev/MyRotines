using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using MyRotines.Application.Services;

namespace MyRotines.Application.Commands;

public class ExtractCommand(ExtractionService extractionService) : AsyncCommand<ExtractCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--file <ZIP>")]
        [Description("Arquivo zip de origem")]
        public string File { get; set; } = string.Empty;

        [CommandOption("--dest <PATH>")]
        [Description("Diretório de destino")]
        public string Destination { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!System.IO.File.Exists(settings.File))
        {
            AnsiConsole.MarkupLine("[red]Arquivo não encontrado[/]");
            return -1;
        }

        if (string.IsNullOrWhiteSpace(settings.Destination))
        {
            AnsiConsole.MarkupLine("[red]Destino é obrigatório[/]");
            return -1;
        }

        await extractionService.ExtractAsync(settings.File, settings.Destination, cancellationToken);

        AnsiConsole.MarkupLine("[green]Extração concluída![/]");
        return 0;
    }
}
