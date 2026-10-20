namespace Payments.Application.IntegrationEvents;

/// <summary>
/// Published when a payment completes, so Order can mark itself Paid.
/// Wire contract with the Order service - Order defines its own copy
/// (Orders.Application.IntegrationEvents). Keep both in sync by hand.
/// </summary>
public sealed record PaymentCompletedEvent(Guid OrderId, Guid PaymentId, decimal Amount, string Currency, DateTimeOffset OccurredAtUtc);
