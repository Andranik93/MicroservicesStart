using MediatR;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using Payments.Application.Common.Exceptions;
using Payments.Application.IntegrationEvents;
using Payments.Application.Commands.CreatePayment;

namespace Payments.Api.Consumers;

/// <summary>
/// Drains OrderConfirmedEvent from RabbitMQ and turns each one into the same
/// CreatePaymentCommand that POST /payments uses - this consumer and that
/// HTTP endpoint are two different adapters into the same Application layer.
/// Nothing in Payments.Application or Payments.Infrastructure needed to
/// change for this: consuming a message and calling an existing command is a
/// hosting-level concern, not a use case of its own.
///
/// Unlike Order's publisher (a new connection per publish), this holds ONE
/// long-lived connection and channel for the service's whole lifetime - a
/// consumer has no choice, it must stay connected to receive messages as
/// they arrive.
/// </summary>
public sealed class OrderConfirmedConsumer(
    IServiceScopeFactory scopeFactory,
    ConnectionFactory connectionFactory,
    ILogger<OrderConfirmedConsumer> logger) : BackgroundService
{
    private const string Exchange = "orders.events";
    private const string RoutingKey = "order.confirmed";
    private const string QueueName = "payment-service.order-confirmed-queue";

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Two separately-started containers, no shared startup order - this
        // service might come up before RabbitMQ is reachable. A basic retry
        // loop here is a different, more fundamental concern than Phase 3's
        // Polly-based resilience: it's "wait for a dependency to exist,"
        // not "handle a flaky call to one that does."
        IConnection? connection = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await connectionFactory.CreateConnectionAsync(stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not connect to RabbitMQ yet, retrying in 3s.");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        if (connection is null)
            return; // stoppingToken was cancelled before a connection succeeded

        _channel = await connection.CreateChannelAsync();

        // Both sides declare the topology defensively - whichever of Order or
        // Payment starts first, the exchange/queue/binding end up the same.
        await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(QueueName, Exchange, RoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer);

        logger.LogInformation("Listening for {Event} on queue {Queue}.", nameof(OrderConfirmedEvent), QueueName);

        // BasicConsumeAsync wires up the subscription and returns immediately.
        // Without this, ExecuteAsync would return and the host would consider
        // this service "done" and tear the connection down.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var @event = JsonSerializer.Deserialize<OrderConfirmedEvent>(json)
                ?? throw new JsonException("Message body deserialized to null.");

            // OccurredAtUtc is when Order actually confirmed - not when we
            // happen to be handling it now. The difference is processing lag:
            // normally milliseconds, but it can grow under load, during a
            // RabbitMQ outage, or on a redelivery long after the original event.
            var lag = DateTimeOffset.UtcNow - @event.OccurredAtUtc;
            logger.LogInformation(
                "Received {Event} for order {OrderId}: occurred at {OccurredAtUtc}, processing lag {LagMs}ms.",
                nameof(OrderConfirmedEvent), @event.OrderId, @event.OccurredAtUtc, lag.TotalMilliseconds);

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            try
            {
                await mediator.Send(new CreatePaymentCommand(@event.OrderId, @event.Amount, @event.Currency));
            }
            catch (ConflictException)
            {
                // A payment already exists for this order - this message is a
                // redelivery (RabbitMQ only guarantees AT LEAST ONCE), not a bug.
                logger.LogInformation(
                    "Payment for order {OrderId} already exists; treating this delivery as already processed.",
                    @event.OrderId);
            }

            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            // No dead-letter queue or retry-with-backoff yet - a genuinely poison
            // message would otherwise loop forever. Acking it and logging loudly
            // is the simplest safe default until that's built; a real, named
            // gap, not a silent one.
            logger.LogError(ex, "Failed to process {Event}; acking anyway (no DLQ yet).", nameof(OrderConfirmedEvent));
            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
    }
}
