using System.Text.Json;
using Orders.Application.Abstractions;
using Orders.Application.IntegrationEvents;
using RabbitMQ.Client;

namespace Orders.Infrastructure.Messaging;

/// <summary>
/// Publishes OrderConfirmedEvent to RabbitMQ's "orders.events" topic exchange.
///
/// Opens a new connection and channel per publish, then closes them: publishing
/// happens rarely (once per confirmed order), so "connect, send, close" is worth
/// more here than the performance of a long-lived connection would be, and it
/// sidesteps any question of sharing a channel safely across concurrent
/// requests. A consumer, unlike a publisher, genuinely needs a long-lived
/// connection to receive messages as they arrive - see Payment's
/// OrderConfirmedConsumer (BackgroundService) for that different shape.
/// </summary>
public sealed class RabbitMqEventPublisher(ConnectionFactory connectionFactory) : IEventPublisher
{
    private const string Exchange = "orders.events";
    private const string RoutingKey = "order.confirmed";

    public async Task PublishOrderConfirmedAsync(OrderConfirmedEvent @event, CancellationToken cancellationToken = default)
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

        await channel.BasicPublishAsync(Exchange, RoutingKey, mandatory: false, properties, body);
    }
}
