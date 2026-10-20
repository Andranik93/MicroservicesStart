using MediatR;

namespace Orders.Application.Commands.AddOrderItem;

public sealed record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    decimal UnitPrice,
    int Quantity) : IRequest;
