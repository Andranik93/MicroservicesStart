namespace Payments.Application.IntegrationEvents;

/// <summary>
/// Payment's own copy of the event Order publishes when an order is confirmed.
///
/// This is the WIRE CONTRACT with the Order service: the JSON shape, plus the
/// exchange name and routing key both sides agree on - not a shared C# type.
/// Order defines its own copy (Orders.Application.IntegrationEvents). Keep
/// both copies in sync by hand if this shape ever changes.
/// </summary>
public sealed record OrderConfirmedEvent(Guid OrderId, decimal Amount, string Currency, DateTimeOffset OccurredAtUtc);
