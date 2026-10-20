namespace Payments.Application.Abstractions;

/// <summary>
/// Commits all pending changes in one transaction.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
