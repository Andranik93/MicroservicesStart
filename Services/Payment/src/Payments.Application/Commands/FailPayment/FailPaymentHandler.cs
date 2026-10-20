using MediatR;
using Payments.Domain.PaymentAggregate;
using Payments.Application.Abstractions;
using Payments.Application.Common.Exceptions;
using Payments.Application.IntegrationEvents;

namespace Payments.Application.Commands.FailPayment;

public sealed class FailPaymentHandler(
    IPaymentRepository payments,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher) : IRequestHandler<FailPaymentCommand>
{
    public async Task Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        payment.Fail(request.Reason);

        // Same save-then-publish reasoning as CompletePaymentHandler.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var @event = new PaymentFailedEvent(payment.OrderId, payment.Id, request.Reason, DateTimeOffset.UtcNow);
        await eventPublisher.PublishPaymentFailedAsync(@event, cancellationToken);
    }
}
