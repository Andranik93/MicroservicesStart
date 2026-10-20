using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Queries.GetOrderById;
using Orders.Application.Queries.Mapping;

namespace Orders.Application.Queries.GetOrdersByCustomerId;

public sealed class GetOrdersByCustomerIdHandler(IOrderRepository orders)
    : IRequestHandler<GetOrdersByCustomerIdQuery, IReadOnlyCollection<OrderResponse>>
{
    public async Task<IReadOnlyCollection<OrderResponse>> Handle(
        GetOrdersByCustomerIdQuery request,
        CancellationToken cancellationToken)
    {
        var customerOrders = await orders.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return customerOrders.Select(OrderMapper.ToResponse).ToList();
    }
}
