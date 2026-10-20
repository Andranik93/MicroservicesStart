using MediatR;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using RabbitMQ.Client.Events;
using Orders.Application.IntegrationEvents;
using Orders.Application.Commands.MarkOrderAsPaid;
using Orders.Application.Commands.MarkOrderPaymentFailed;

namespace Orders.Api.Consumers;

/// <summary>
/// Drains Payment's outcome events and turns each one into the matching
/// Order command - the mirror image of Payment's OrderConfirmedConsumer.
///
/// One consumer handles BOTH PaymentCompletedEvent and PaymentFailedEvent,
/// bound with the wildcard routing key "payment.*" - the first real use of
/// topic-exchange pattern matching in this project; "order.confirmed" (the
/// other consumer) is a single literal key, with nothing to match against.
/// The routing key on each delivery says which event it actually is.
/// </summary>
public sealed class PaymentOutcomeConsumer(
    IServiceScopeFactory scopeFactory,
    ConnectionFactory connectionFactory,
    ILogger<PaymentOutcomeConsumer> logger) : BackgroundService
{
    private const string Exchange = "payments.events";
    private const string RoutingKeyPattern = "payment.*";
    private const string QueueName = "order-service.payment-outcome-queue";

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
            return;

        _channel = await connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(QueueName, Exchange, RoutingKeyPattern);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer);

        logger.LogInformation("Listening for payment outcomes on queue {Queue}.", QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            switch (args.RoutingKey)
            {
                case "payment.completed":
                    var completed = JsonSerializer.Deserialize<PaymentCompletedEvent>(json)
                        ?? throw new JsonException("Message body deserialized to null.");
                    LogProcessingLag(nameof(PaymentCompletedEvent), completed.OrderId, completed.OccurredAtUtc);
                    await mediator.Send(new MarkOrderAsPaidCommand(completed.OrderId));
                    break;

                case "payment.failed":
                    var failed = JsonSerializer.Deserialize<PaymentFailedEvent>(json)
                        ?? throw new JsonException("Message body deserialized to null.");
                    LogProcessingLag(nameof(PaymentFailedEvent), failed.OrderId, failed.OccurredAtUtc);
                    await mediator.Send(new MarkOrderPaymentFailedCommand(failed.OrderId, failed.Reason));
                    break;

                default:
                    // Defensive: the binding is "payment.*", so only expected keys
                    // should ever arrive. Ack and move on rather than get stuck.
                    logger.LogWarning("Ignoring message with unexpected routing key {RoutingKey}.", args.RoutingKey);
                    break;
            }

            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            // Same reasoning as Payment's consumer: no dead-letter queue or
            // retry-with-backoff yet. Ack and log loudly rather than loop
            // forever on a poison message - a named gap, not a silent one.
            logger.LogError(ex, "Failed to process payment outcome event; acking anyway (no DLQ yet).");
            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
    }

    // OccurredAtUtc is when Payment actually completed/failed the payment -
    // not when we happen to be handling it now. The difference is processing
    // lag: normally milliseconds, but it can grow under load, during a
    // RabbitMQ outage, or on a redelivery long after the original event.
    private void LogProcessingLag(string eventName, Guid orderId, DateTimeOffset occurredAtUtc)
    {
        var lag = DateTimeOffset.UtcNow - occurredAtUtc;
        logger.LogInformation(
            "Received {Event} for order {OrderId}: occurred at {OccurredAtUtc}, processing lag {LagMs}ms.",
            eventName, orderId, occurredAtUtc, lag.TotalMilliseconds);
    }
}
