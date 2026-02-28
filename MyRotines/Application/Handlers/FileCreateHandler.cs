using Microsoft.Extensions.Logging;
using MyRotines.Application.Events;
using MyRotines.Domain.Events;

namespace MyRotines.Application.Handlers;

public sealed class FileCreateHandler(ILogger<FileCreateHandler> _logger) : IEventHandler<FileCreateEvent>
{
    public async Task HandleAsync(FileCreateEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);

        try
        {
            _logger.LogInformation("File created: {FullPath} at {OccuredAt}", @event.FullPath, @event.OccuredAt);
            _logger.LogInformation("File processing...");
            _logger.LogInformation("File processing...");
            _logger.LogInformation("File processing finaly.");
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "File processing error");
        }
    }
}