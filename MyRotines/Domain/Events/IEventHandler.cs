namespace MyRotines.Domain.Events;

public interface IEventHandler;

/// <summary>
/// Base interface for typed event handlers
/// </summary>
/// <typeparam name="TEvent">Type of event handled by this handler</typeparam>
public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IEvent
{
	Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}