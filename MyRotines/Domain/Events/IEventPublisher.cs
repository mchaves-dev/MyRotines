namespace MyRotines.Domain.Events;

public interface IEventPublisher
{
	Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}
