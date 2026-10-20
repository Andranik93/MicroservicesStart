using Emails.Domain.EmailAggregate;

namespace Emails.Application.Abstractions;

public interface IEmailRepository
{
    Task<bool> ExistsAsync(Guid orderId, NotificationKind kind, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SentEmail>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task AddAsync(SentEmail email, CancellationToken cancellationToken = default);
}
