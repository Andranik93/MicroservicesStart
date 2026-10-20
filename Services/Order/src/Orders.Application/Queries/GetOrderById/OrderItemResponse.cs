namespace Orders.Application.Queries.GetOrderById;

public sealed record OrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity);
