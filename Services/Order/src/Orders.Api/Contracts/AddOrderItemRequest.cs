namespace Orders.Api.Contracts;

public sealed record AddOrderItemRequest(Guid ProductId, decimal UnitPrice, int Quantity);
