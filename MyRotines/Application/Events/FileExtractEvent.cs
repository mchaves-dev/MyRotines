using MyRotines.Domain.Events;

namespace MyRotines.Application.Events;

public record FileExtractEvent(string FilePath) : IEvent;
