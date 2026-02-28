using MyRotines.Domain.Events;

namespace MyRotines.Application.Events;

public record FileDownloadedEvent(string FilePath, bool Extract) : IEvent;
