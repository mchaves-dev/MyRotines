using Microsoft.Extensions.Logging;
using MyRotines.Application.Events;
using MyRotines.Application.Services;
using MyRotines.Domain.Events;

namespace MyRotines.Application.Handlers;

public sealed class FileExtractHandler(ILogger<FileExtractHandler> _logger, ArquiveExtractService _arquiveExtractService) : IEventHandler<FileExtractEvent>
{
    public async Task HandleAsync(FileExtractEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "FileDownloadedEvent received. Path: {FilePath}, Extract: {Extract}",
            @event.FilePath);

        var directory = Path.GetDirectoryName(@event.FilePath);

        if (string.IsNullOrWhiteSpace(directory))
        {
            _logger.LogWarning("Cannot determine directory for file {FilePath}", @event.FilePath);
            return;
        }

        var destination = Path.Combine(
            directory,
            Path.GetFileNameWithoutExtension(@event.FilePath));

        _logger.LogInformation(
            "Starting extraction. Source: {Source}, Destination: {Destination}",
            @event.FilePath,
            destination);

        await _arquiveExtractService.ExtractAsync(
            @event.FilePath,
            destination,
            cancellationToken);

        _logger.LogInformation(
            "Extraction completed successfully for {FilePath}",
            @event.FilePath);
    }
}