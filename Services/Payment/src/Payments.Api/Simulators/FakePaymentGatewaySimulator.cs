using MediatR;
using Payments.Application.Abstractions;
using Payments.Application.Commands.CompletePayment;
using Payments.Application.Commands.FailPayment;

namespace Payments.Api.Simulators;

/// <summary>
/// Stands in for a real payment gateway (Stripe, PayPal, ...), which this
/// project never integrates with. Polls for Pending payments and, after a
/// simulated processing delay, randomly approves or declines each one by
/// sending the SAME CompletePaymentCommand/FailPaymentCommand the HTTP
/// endpoints already use - no new Application logic, just a different,
/// automatic caller.
///
/// This is a deliberate, named stand-in - same category as the old
/// synchronous Order-to-Payment HTTP call was before messaging replaced it.
/// Toggle it off (FakePaymentGateway:Enabled = false) the moment a real
/// gateway integration exists.
/// </summary>
public sealed class FakePaymentGatewaySimulator(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<FakePaymentGatewaySimulator> logger) : BackgroundService
{
    private readonly TimeSpan _processingDelay = TimeSpan.FromSeconds(
        configuration.GetValue("FakePaymentGateway:ProcessingDelaySeconds", 5));

    private readonly double _successRate =
        configuration.GetValue("FakePaymentGateway:SuccessRate", 0.8);

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Fake payment gateway running: {Delay}s simulated processing, {SuccessRate:P0} approval rate.",
            _processingDelay.TotalSeconds, _successRate);

        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessDuePaymentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fake payment gateway tick failed.");
            }
        }
    }

    private async Task ProcessDuePaymentsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var payments = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        var cutoff = DateTimeOffset.UtcNow - _processingDelay;
        var duePayments = await payments.GetPendingCreatedBeforeAsync(cutoff, cancellationToken);

        foreach (var payment in duePayments)
        {
            if (Random.Shared.NextDouble() < _successRate)
            {
                logger.LogInformation(
                    "Fake gateway: approving payment {PaymentId} for order {OrderId}.",
                    payment.Id, payment.OrderId);
                await mediator.Send(new CompletePaymentCommand(payment.Id), cancellationToken);
            }
            else
            {
                logger.LogInformation(
                    "Fake gateway: declining payment {PaymentId} for order {OrderId}.",
                    payment.Id, payment.OrderId);
                await mediator.Send(new FailPaymentCommand(payment.Id, "simulated_decline"), cancellationToken);
            }
        }
    }
}
