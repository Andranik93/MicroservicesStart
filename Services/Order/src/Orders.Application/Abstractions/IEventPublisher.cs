using Orders.Application.IntegrationEvents;

namespace Orders.Application.Abstractions;

/// <summary>
/// Publishes Order's integration events. Defined here (dependency inversion),
/// implemented in Infrastructure with RabbitMQ - the Application layer knows
/// nothing about message brokers.
///
/// One specific method rather than a generic Publish&lt;TEvent&gt;: there is
/// only one event so far. Generalizing this now, before a second event shows
/// what the real variation looks like, would be guessing at an abstraction.
/// </summary>
public interface IEventPublisher
{
    Task PublishOrderConfirmedAsync(OrderConfirmedEvent @event, CancellationToken cancellationToken = default);
}
