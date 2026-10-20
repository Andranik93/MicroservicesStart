using Emails.Domain.Exceptions;

namespace Emails.Domain.EmailAggregate;

/// <summary>
/// A different shape of aggregate than Order or Payment: this isn't a
/// multi-step workflow with a state machine, it's a record that one
/// notification was sent - an audit/idempotency log entry, not a process.
/// The actual email content isn't stored here; it lives in the SMTP message
/// itself (visible in Mailpit). This row only exists so a redelivered event
/// doesn't send the same email twice.
/// </summary>
public sealed class SentEmail
{
    public Guid Id { get; }
    public Guid OrderId { get; }
    public NotificationKind Kind { get; }
    public string ToAddress { get; } = null!;
    public string Subject { get; } = null!;
    public DateTimeOffset SentAtUtc { get; }

    private SentEmail(Guid id, Guid orderId, NotificationKind kind, string toAddress, string subject, DateTimeOffset sentAtUtc)
    {
        Id = id;
        OrderId = orderId;
        Kind = kind;
        ToAddress = toAddress;
        Subject = subject;
        SentAtUtc = sentAtUtc;
    }

    public static SentEmail Create(Guid orderId, NotificationKind kind, string toAddress, string subject)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId is required.");

        if (string.IsNullOrWhiteSpace(toAddress))
            throw new DomainException("ToAddress is required.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Subject is required.");

        return new SentEmail(Guid.NewGuid(), orderId, kind, toAddress, subject, DateTimeOffset.UtcNow);
    }
}
