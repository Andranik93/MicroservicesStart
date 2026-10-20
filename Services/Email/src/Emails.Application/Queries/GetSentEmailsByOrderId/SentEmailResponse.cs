namespace Emails.Application.Queries.GetSentEmailsByOrderId;

public sealed record SentEmailResponse(Guid Id, Guid OrderId, string Kind, string ToAddress, string Subject, DateTimeOffset SentAtUtc);
