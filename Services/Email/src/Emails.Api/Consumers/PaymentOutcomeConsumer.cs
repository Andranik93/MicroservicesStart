using System.Text;
using System.Text.Json;
using Emails.Application.Commands.SendOrderPaidEmail;
using Emails.Application.Commands.SendPaymentFailedEmail;
using Emails.Application.IntegrationEvents;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Emails.Api.Consumers;

/// <summary>
/// Drains the SAME "payments.events" exchange, with the SAME "payment.*"
/// wildcard pattern, that Order's PaymentOutcomeConsumer uses - but through
/// its OWN queue ("email-service.payment-outcome-queue"). This is the actual
/// payoff of choosing a topic exchange back when there was only one
/// consumer: two independent queues bound to one exchange each get their
/// OWN full copy of every matching message. If this is one queue shared
/// between Order and Email instead, each message would go to only ONE of
/// them (competing consumers) - wrong here, since both services must react
/// to every event independently.
/// </summary>
public sealed class PaymentOutcomeConsumer(
    IServiceScopeFactory scopeFactory,
    ConnectionFactory connectionFactory,
    ILogger<PaymentOutcomeConsumer> logger) : BackgroundService
{
    private const string Exchange = "payments.events";
    private const string RoutingKeyPattern = "payment.*";
    private const string QueueName = "email-service.payment-outcome-queue";

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
                    await mediator.Send(new SendOrderPaidEmailCommand(completed.OrderId));
                    break;

                case "payment.failed":
                    var failed = JsonSerializer.Deserialize<PaymentFailedEvent>(json)
                        ?? throw new JsonException("Message body deserialized to null.");
                    await mediator.Send(new SendPaymentFailedEmailCommand(failed.OrderId, failed.Reason));
                    break;

                default:
                    logger.LogWarning("Ignoring message with unexpected routing key {RoutingKey}.", args.RoutingKey);
                    break;
            }

            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            // Same reasoning as the other consumers: no dead-letter queue or
            // retry-with-backoff yet. Ack and log loudly rather than loop
            // forever on a poison message - a named gap, not a silent one.
            logger.LogError(ex, "Failed to process payment outcome event; acking anyway (no DLQ yet).");
            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
    }
}
