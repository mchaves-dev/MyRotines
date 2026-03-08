using Microsoft.Extensions.Logging;
using MyRotines.Application.Events;
using MyRotines.Application.Services;
using MyRotines.Domain.Events;

namespace MyRotines.Application.Handlers;

public sealed class FileDownloadedHandler(
    ILogger<FileDownloadedHandler> logger,
    ExtractionService extractionService,
    FileMoveService fileMoveService) : IEventHandler<FileDownloadedEvent>
{
    public async Task HandleAsync(FileDownloadedEvent @event, CancellationToken cancellationToken)
    {
        string? extractedPath = null;

        if (@event.Extract)
        {
            var directory = Path.GetDirectoryName(@event.FilePath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                logger.LogWarning("Cannot determine directory for file {FilePath}", @event.FilePath);
                return;
            }

            extractedPath = Path.Combine(directory, Path.GetFileNameWithoutExtension(@event.FilePath));

            logger.LogInformation("Starting extraction. Source: {Source}, Destination: {Destination}", @event.FilePath, extractedPath);

            await extractionService.ExtractAsync(@event.FilePath, extractedPath, cancellationToken);

            if (@event.CleanupDownloadedFile && File.Exists(@event.FilePath))
            {
                File.Delete(@event.FilePath);
                logger.LogInformation("Downloaded zip removed: {FilePath}", @event.FilePath);
            }
        }

        if (string.IsNullOrWhiteSpace(@event.MoveToDirectory))
        {
            return;
        }

        var sourcePath = extractedPath ?? @event.FilePath;

        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
        {
            logger.LogWarning("Source path not found for move operation: {SourcePath}", sourcePath);
            return;
        }

        var targetPath = await fileMoveService.MoveAsync(sourcePath, @event.MoveToDirectory, overwrite: true, cancellationToken);

        logger.LogInformation("Path moved to organized destination: {TargetPath}", targetPath);
    }
}
