using MyRotines.Application.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MyRotines.Application.Commands;


public class ExtractCommand(ArquiveExtractService _arquiveExtractService) : AsyncCommand<ExtractCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--file <ZIP>")]
        public string File { get; set; } = string.Empty;

        [CommandOption("--dest <PATH>")]
        public string Destination { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!System.IO.File.Exists(settings.File))
        {
            AnsiConsole.MarkupLine("[red]Arquivo não encontrado[/]");
            return -1;
        }

        await Task.Run(() =>
        {
            _arquiveExtractService.ExtractAsync(settings.File, settings.Destination, cancellationToken);
        });

        AnsiConsole.MarkupLine("[green]Extração concluída![/]");
        return 0;
    }
}