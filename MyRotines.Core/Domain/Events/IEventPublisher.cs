namespace MyRotines.Core.Domain.Events;

public interface IEventPublisher
{
	Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}
