namespace Orders.Application.IntegrationEvents;

/// <summary>
/// Published when an order is confirmed, so Payment can open a payment attempt.
///
/// This is the WIRE CONTRACT with the Payment service: the JSON shape, plus the
/// exchange name and routing key the publisher/consumer agree on - not a shared
/// C# type. Payment defines its own copy of this record. Keep both copies in
/// sync by hand if this shape ever changes; that manual-sync cost is the trade
/// for not coupling two independently-deployable services to one assembly.
/// </summary>
public sealed record OrderConfirmedEvent(Guid OrderId, decimal Amount, string Currency, DateTimeOffset OccurredAtUtc);
