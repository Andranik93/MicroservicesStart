namespace Payments.Application.IntegrationEvents;

/// <summary>
/// Published when a payment fails, so Order can record the failure.
/// Wire contract with the Order service - Order defines its own copy
/// (Orders.Application.IntegrationEvents). Keep both in sync by hand.
/// </summary>
public sealed record PaymentFailedEvent(Guid OrderId, Guid PaymentId, string Reason, DateTimeOffset OccurredAtUtc);
