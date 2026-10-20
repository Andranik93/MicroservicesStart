using System.Text.Json;
using Payments.Application.Abstractions;
using Payments.Application.IntegrationEvents;
using RabbitMQ.Client;

namespace Payments.Infrastructure.Messaging;

/// <summary>
/// Publishes PaymentCompletedEvent/PaymentFailedEvent to the "payments.events"
/// topic exchange, with routing keys "payment.completed" / "payment.failed".
///
/// Same per-call connect-and-close shape as Order's RabbitMqEventPublisher,
/// for the same reason: publishing happens rarely (once per payment outcome).
/// </summary>
public sealed class RabbitMqEventPublisher(ConnectionFactory connectionFactory) : IEventPublisher
{
    private const string Exchange = "payments.events";

    public Task PublishPaymentCompletedAsync(PaymentCompletedEvent @event, CancellationToken cancellationToken = default) =>
        PublishAsync("payment.completed", @event, cancellationToken);

    public Task PublishPaymentFailedAsync(PaymentFailedEvent @event, CancellationToken cancellationToken = default) =>
        PublishAsync("payment.failed", @event, cancellationToken);

    private async Task PublishAsync<TEvent>(string routingKey, TEvent @event, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(Exchange, routingKey, mandatory: false, properties, body);
    }
}
