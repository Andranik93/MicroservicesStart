using MediatR;
using Orders.Domain.OrderAggregate;
using Orders.Application.Abstractions;
using Orders.Application.Common.Exceptions;

namespace Orders.Application.Commands.MarkOrderPaymentFailed;

public sealed class MarkOrderPaymentFailedHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : IRequestHandler<MarkOrderPaymentFailedCommand>
{
    public async Task Handle(MarkOrderPaymentFailedCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // Same idempotent-redelivery reasoning as MarkOrderAsPaidHandler.
        if (order.Status == OrderStatus.PaymentFailed)
            return;

        order.MarkPaymentFailed(request.Reason);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
