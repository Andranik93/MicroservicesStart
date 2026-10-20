using MediatR;

namespace Orders.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(Guid CustomerId, string Currency) : IRequest<Guid>;
