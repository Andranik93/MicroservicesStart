namespace Orders.Api.Contracts;

public sealed record CreateOrderRequest(Guid CustomerId, string Currency);
