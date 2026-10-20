using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Queries.Mapping;

namespace Orders.Application.Queries.GetOrderById;

public sealed class GetOrderByIdHandler(IOrderRepository orders)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken);

        // Unlike a command, a missing order here is a normal outcome, not an error:
        // the caller only asked whether it exists. The endpoint turns this into 404.
        return order is null ? null : OrderMapper.ToResponse(order);
    }
}
