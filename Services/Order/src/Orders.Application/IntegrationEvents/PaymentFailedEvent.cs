namespace Orders.Application.IntegrationEvents;

/// <summary>
/// Order's own copy of the event Payment publishes when a payment fails.
/// Wire contract with the Payment service - Payment defines its own copy
/// (Payments.Application.IntegrationEvents). Keep both in sync by hand.
/// </summary>
public sealed record PaymentFailedEvent(Guid OrderId, Guid PaymentId, string Reason, DateTimeOffset OccurredAtUtc);
