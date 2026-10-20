using Payments.Application.IntegrationEvents;

namespace Payments.Application.Abstractions;

/// <summary>
/// Publishes Payment's integration events. Defined here (dependency
/// inversion), implemented in Infrastructure with RabbitMQ.
/// </summary>
public interface IEventPublisher
{
    Task PublishPaymentCompletedAsync(PaymentCompletedEvent @event, CancellationToken cancellationToken = default);

    Task PublishPaymentFailedAsync(PaymentFailedEvent @event, CancellationToken cancellationToken = default);
}
