using MyRotines.Application.Events;
using MyRotines.Application.Services;
using MyRotines.Domain.Events;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public class DownloadCommand(ArquiveDownloadService _arquiveDownloadService, IEventPublisher _eventPublisher) : AsyncCommand<DownloadCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--url <URL>")]
        [Description("URL do arquivo")]
        public string Url { get; set; } = string.Empty;

        [CommandOption("--output <PATH>")]
        [Description("Caminho de destino")]
        public string Output { get; set; } = string.Empty;

        [CommandOption("--extract")]
        public bool Extract { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            AnsiConsole.MarkupLine("[red]URL é obrigatória[/]");
            return -1;
        }

        await AnsiConsole.Status()
             .StartAsync("Baixando arquivo...", async ctx =>
             {
                 await _arquiveDownloadService.DownloadAsync(settings.Url, settings.Output, cancellationToken);
                 if (settings.Extract)
                 {
                     await _eventPublisher.PublishAsync(new FileDownloadedEvent(settings.Output, settings.Extract), cancellationToken);
                 }
             });

        AnsiConsole.MarkupLine("[green]Download concluído![/]");
        return 0;
    }
}