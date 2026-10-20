using Orders.Application.Abstractions;

namespace Orders.Infrastructure.Persistence;

public sealed class UnitOfWork(OrdersDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
