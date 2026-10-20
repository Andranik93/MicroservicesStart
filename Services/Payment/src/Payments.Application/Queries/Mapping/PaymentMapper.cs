using Payments.Application.Queries.GetPaymentById;
using Payments.Domain.PaymentAggregate;

namespace Payments.Application.Queries.Mapping;

/// <summary>
/// Shared Payment -> PaymentResponse mapping, used by every query that returns a payment.
/// </summary>
internal static class PaymentMapper
{
    public static PaymentResponse ToResponse(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Status.ToString(),
        payment.FailureReason);
}
