using Payments.Domain.PaymentAggregate;

namespace Payments.Application.Abstractions;

/// <summary>
/// Persistence contract defined by the Application layer. Infrastructure implements it,
/// so the use cases never depend on EF Core.
/// </summary>
public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    // Internal plumbing for FakePaymentGatewaySimulator - not a user-facing read,
    // so it's a plain repository method rather than a MediatR query.
    Task<IReadOnlyCollection<Payment>> GetPendingCreatedBeforeAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}
