using MyRotines.Application.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public class MoveCommand(FileMoveService fileMoveService) : AsyncCommand<MoveCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--source <PATH>")]
        [Description("Arquivo ou pasta de origem")]
        public string Source { get; set; } = string.Empty;

        [CommandOption("--dest <PATH>")]
        [Description("Diretório destino")]
        public string Destination { get; set; } = string.Empty;

        [CommandOption("--no-overwrite")]
        [Description("Não sobrescrever quando existir destino")]
        public bool NoOverwrite { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Source) || string.IsNullOrWhiteSpace(settings.Destination))
        {
            AnsiConsole.MarkupLine("[red]Source e Dest são obrigatórios[/]");
            return -1;
        }

        var targetPath = await fileMoveService.MoveAsync(
            settings.Source,
            settings.Destination,
            overwrite: !settings.NoOverwrite,
            cancellationToken);

        AnsiConsole.MarkupLine($"[green]Movido para:[/] {targetPath}");
        return 0;
    }
}
