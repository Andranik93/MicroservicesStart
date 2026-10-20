using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Common.Exceptions;
using Orders.Domain.OrderAggregate;
using Orders.Domain.ValueObjects;

namespace Orders.Application.Commands.AddOrderItem;

public sealed class AddOrderItemHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : IRequestHandler<AddOrderItemCommand>
{
    public async Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // The price takes the order's currency, so the command doesn't have to repeat it.
        var unitPrice = Money.Create(request.UnitPrice, order.Currency);

        order.AddItem(request.ProductId, unitPrice, request.Quantity);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
