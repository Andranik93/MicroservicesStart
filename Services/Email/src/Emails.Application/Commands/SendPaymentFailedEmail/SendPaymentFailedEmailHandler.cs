using Emails.Application.Abstractions;
using Emails.Domain.EmailAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emails.Application.Commands.SendPaymentFailedEmail;

public sealed class SendPaymentFailedEmailHandler(
    IEmailRepository emails,
    IUnitOfWork unitOfWork,
    IEmailSender emailSender,
    IOrderServiceClient orderServiceClient,
    ILogger<SendPaymentFailedEmailHandler> logger) : IRequestHandler<SendPaymentFailedEmailCommand>
{
    public async Task Handle(SendPaymentFailedEmailCommand request, CancellationToken cancellationToken)
    {
        if (await emails.ExistsAsync(request.OrderId, NotificationKind.PaymentFailed, cancellationToken))
            return;

        var order = await orderServiceClient.GetOrderAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            logger.LogWarning("Could not load order {OrderId} to send its payment-failed email; skipping.", request.OrderId);
            return;
        }

        var toAddress = $"customer-{order.CustomerId}@example.test";
        const string subject = "Your payment could not be processed";
        var body = $"We could not process payment of {order.Total:F2} {order.Currency} for order {order.Id}. Reason: {request.Reason}.";

        await emailSender.SendAsync(toAddress, subject, body, cancellationToken);

        var sentEmail = SentEmail.Create(request.OrderId, NotificationKind.PaymentFailed, toAddress, subject);
        await emails.AddAsync(sentEmail, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
