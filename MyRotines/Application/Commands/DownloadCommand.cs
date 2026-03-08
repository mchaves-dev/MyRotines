using MyRotines.Application.Events;
using MyRotines.Application.Services;
using MyRotines.Domain.Events;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace MyRotines.Application.Commands;

public class DownloadCommand(DownloadService downloadService, IEventPublisher eventPublisher) : AsyncCommand<DownloadCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--url <URL>")]
        [Description("URL do arquivo")]
        public string Url { get; set; } = string.Empty;

        [CommandOption("--output <PATH>")]
        [Description("Caminho completo do arquivo de destino")]
        public string Output { get; set; } = string.Empty;

        [CommandOption("--extract")]
        [Description("Extrai automaticamente após o download")]
        public bool Extract { get; set; }

        [CommandOption("--move-to <PATH>")]
        [Description("Diretório final para organização do arquivo/pasta")]
        public string? MoveTo { get; set; }

        [CommandOption("--cleanup-downloaded")]
        [Description("Remove o .zip após extrair")]
        public bool CleanupDownloadedFile { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            AnsiConsole.MarkupLine("[red]URL é obrigatória[/]");
            return -1;
        }

        if (string.IsNullOrWhiteSpace(settings.Output))
        {
            AnsiConsole.MarkupLine("[red]Output é obrigatório[/]");
            return -1;
        }

        await AnsiConsole.Status().StartAsync("Baixando arquivo...", async _ =>
        {
            await downloadService.DownloadAsync(settings.Url, settings.Output, cancellationToken);

            await eventPublisher.PublishAsync(
                new FileDownloadedEvent(
                    settings.Output,
                    settings.Extract,
                    settings.MoveTo,
                    settings.CleanupDownloadedFile),
                cancellationToken);
        });

        AnsiConsole.MarkupLine("[green]Download concluído![/]");
        return 0;
    }
}
