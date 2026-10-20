using MediatR;
using Orders.Application.Queries.GetOrderById;

namespace Orders.Application.Queries.GetOrdersByCustomerId;

public sealed record GetOrdersByCustomerIdQuery(Guid CustomerId) : IRequest<IReadOnlyCollection<OrderResponse>>;
