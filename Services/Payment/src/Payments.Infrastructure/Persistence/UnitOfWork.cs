using Payments.Application.Abstractions;

namespace Payments.Infrastructure.Persistence;

public sealed class UnitOfWork(PaymentsDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
