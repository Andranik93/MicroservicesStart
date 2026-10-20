using Emails.Application.Abstractions;
using Emails.Domain.EmailAggregate;
using Emails.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Emails.Infrastructure.Repositories;

public sealed class EmailRepository(EmailsDbContext dbContext) : IEmailRepository
{
    public Task<bool> ExistsAsync(Guid orderId, NotificationKind kind, CancellationToken cancellationToken = default) =>
        dbContext.SentEmails.AnyAsync(e => e.OrderId == orderId && e.Kind == kind, cancellationToken);

    public async Task<IReadOnlyCollection<SentEmail>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        await dbContext.SentEmails
            .Where(e => e.OrderId == orderId)
            .OrderBy(e => e.SentAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SentEmail email, CancellationToken cancellationToken = default) =>
        await dbContext.SentEmails.AddAsync(email, cancellationToken);
}
