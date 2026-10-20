using Emails.Application.Abstractions;
using Emails.Domain.EmailAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emails.Application.Commands.SendOrderPaidEmail;

public sealed class SendOrderPaidEmailHandler(
    IEmailRepository emails,
    IUnitOfWork unitOfWork,
    IEmailSender emailSender,
    IOrderServiceClient orderServiceClient,
    ILogger<SendOrderPaidEmailHandler> logger) : IRequestHandler<SendOrderPaidEmailCommand>
{
    public async Task Handle(SendOrderPaidEmailCommand request, CancellationToken cancellationToken)
    {
        // Idempotent no-op on redelivery - same reasoning as Order's
        // MarkOrderAsPaidHandler: RabbitMQ only guarantees AT LEAST ONCE.
        if (await emails.ExistsAsync(request.OrderId, NotificationKind.OrderPaid, cancellationToken))
            return;

        var order = await orderServiceClient.GetOrderAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            // A failed enrichment READ is not worth blocking or retrying hard
            // over - log it and move on. No DLQ/retry yet, same named gap as
            // the consumers calling this handler.
            logger.LogWarning("Could not load order {OrderId} to send its paid email; skipping.", request.OrderId);
            return;
        }

        // No real customer profile/email store exists anywhere in this system -
        // same deliberate simulation as FakePaymentGatewaySimulator.
        var toAddress = $"customer-{order.CustomerId}@example.test";
        const string subject = "Your order has been paid";
        var body = $"Thanks! We received payment of {order.Total:F2} {order.Currency} for order {order.Id}.";

        await emailSender.SendAsync(toAddress, subject, body, cancellationToken);

        var sentEmail = SentEmail.Create(request.OrderId, NotificationKind.OrderPaid, toAddress, subject);
        await emails.AddAsync(sentEmail, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
