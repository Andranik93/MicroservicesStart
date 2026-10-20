using MediatR;
using Payments.Domain.PaymentAggregate;
using Payments.Application.Abstractions;
using Payments.Application.Common.Exceptions;
using Payments.Application.IntegrationEvents;

namespace Payments.Application.Commands.CompletePayment;

public sealed class CompletePaymentHandler(
    IPaymentRepository payments,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher) : IRequestHandler<CompletePaymentCommand>
{
    public async Task Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await payments.GetByIdAsync(request.PaymentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), request.PaymentId);

        payment.Complete();

        // Save first, then publish - same reasoning as Order's ConfirmOrderHandler:
        // a published message can't be "rolled back," so we only tell the world
        // after our own state is durably committed.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var @event = new PaymentCompletedEvent(
            payment.OrderId, payment.Id, payment.Amount.Amount, payment.Amount.Currency, DateTimeOffset.UtcNow);
        await eventPublisher.PublishPaymentCompletedAsync(@event, cancellationToken);
    }
}
