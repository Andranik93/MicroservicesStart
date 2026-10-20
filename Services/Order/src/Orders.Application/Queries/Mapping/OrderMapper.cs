using Orders.Domain.OrderAggregate;
using Orders.Application.Queries.GetOrderById;

namespace Orders.Application.Queries.Mapping;

/// <summary>
/// Shared Order -> OrderResponse mapping, used by every query that returns an order.
/// </summary>
internal static class OrderMapper
{
    public static OrderResponse ToResponse(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Currency,
        order.Status.ToString(),
        order.Total.Amount,
        order.PaymentFailureReason,
        order.Items
            .Select(i => new OrderItemResponse(i.ProductId, i.UnitPrice.Amount, i.Quantity))
            .ToList());
}
