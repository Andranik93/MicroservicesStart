namespace Emails.Application.IntegrationEvents;

/// <summary>
/// Email's own copy of the event Payment publishes when a payment completes.
/// Wire contract with the Payment service - Payment defines its own copy
/// (Payments.Application.IntegrationEvents). Keep both in sync by hand.
/// </summary>
public sealed record PaymentCompletedEvent(Guid OrderId, Guid PaymentId, decimal Amount, string Currency, DateTimeOffset OccurredAtUtc);
