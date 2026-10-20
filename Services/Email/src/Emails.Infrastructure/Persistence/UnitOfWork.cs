using Emails.Application.Abstractions;

namespace Emails.Infrastructure.Persistence;

public sealed class UnitOfWork(EmailsDbContext dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
