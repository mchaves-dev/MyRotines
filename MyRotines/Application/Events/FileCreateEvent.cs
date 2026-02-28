using MyRotines.Domain.Events;

namespace MyRotines.Application.Events;

public sealed record FileCreateEvent(string FullPath, DateTime OccuredAt) : IEvent;
