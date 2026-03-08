using MyRotines.Domain.Events;

namespace MyRotines.Application.Events;

public sealed record FileDownloadedEvent(
    string FilePath,
    bool Extract,
    string? MoveToDirectory,
    bool CleanupDownloadedFile) : IEvent;
