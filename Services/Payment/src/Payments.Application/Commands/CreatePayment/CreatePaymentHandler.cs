using MediatR;
using Payments.Domain.ValueObjects;
using Payments.Domain.PaymentAggregate;
using Payments.Application.Abstractions;
using Payments.Application.Common.Exceptions;

namespace Payments.Application.Commands.CreatePayment;

public sealed class CreatePaymentHandler(IPaymentRepository payments, IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Business rule: one payment per order. This is a conflict (409), not a
        // validation error (400) - the request itself is well-formed, it just
        // can't be applied given what already exists.
        var existing = await payments.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (existing is not null)
            throw new ConflictException($"Order '{request.OrderId}' already has a payment.");

        var payment = Payment.Create(request.OrderId, Money.Create(request.Amount, request.Currency));

        await payments.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
