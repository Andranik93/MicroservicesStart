using Payments.Domain.Exceptions;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.PaymentAggregate;

/// <summary>
/// A single payment attempt for one order. The Payment service only knows an
/// OrderId as a plain Guid - it never queries the Order service's database.
/// Each service owns its own data.
/// </summary>
public sealed class Payment
{
    public Guid Id { get; }
    public Guid OrderId { get; }
    public Money Amount { get; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    private Payment(Guid id, Guid orderId, Money amount, DateTimeOffset createdAtUtc)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public static Payment Create(Guid orderId, Money amount)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId is required.");

        return new Payment(Guid.NewGuid(), orderId, amount, DateTimeOffset.UtcNow);
    }

    public void Complete()
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Only a pending payment can be completed. Current status: {Status}.");

        Status = PaymentStatus.Completed;
    }

    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException($"Only a pending payment can fail. Current status: {Status}.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A failure reason is required.");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }

    // EF Core materializes a blank instance through this constructor, then
    // writes every property straight to its backing field, since none of
    // them has a public or internal setter. Never called by domain code -
    // the "= null!" above only silences the nullable warning for this path.
    private Payment()
    {
    }
}
