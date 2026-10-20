using Microsoft.EntityFrameworkCore;
using Payments.Application.Abstractions;
using Payments.Domain.PaymentAggregate;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Repositories;

public sealed class PaymentRepository(PaymentsDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

    public async Task<IReadOnlyCollection<Payment>> GetPendingCreatedBeforeAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken = default) =>
        await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAtUtc <= cutoffUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await dbContext.Payments.AddAsync(payment, cancellationToken);
}
