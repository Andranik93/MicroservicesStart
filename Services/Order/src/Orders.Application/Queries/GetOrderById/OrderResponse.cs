namespace Orders.Application.Queries.GetOrderById;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Currency,
    string Status,
    decimal Total,
    string? PaymentFailureReason,
    IReadOnlyCollection<OrderItemResponse> Items);
