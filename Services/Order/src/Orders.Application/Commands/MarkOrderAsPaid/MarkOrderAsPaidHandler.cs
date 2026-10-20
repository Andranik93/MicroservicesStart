using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Common.Exceptions;
using Orders.Domain.OrderAggregate;

namespace Orders.Application.Commands.MarkOrderAsPaid;

public sealed class MarkOrderAsPaidHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : IRequestHandler<MarkOrderAsPaidCommand>
{
    public async Task Handle(MarkOrderAsPaidCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        // Idempotent no-op if this is a redelivery of a message we already
        // handled - RabbitMQ only guarantees AT LEAST ONCE. Checked here,
        // explicitly, rather than catching the DomainException MarkAsPaid
        // would throw for ANY wrong status: that would also swallow a
        // genuinely different bug (e.g. the order was never confirmed).
        if (order.Status == OrderStatus.Paid)
            return;

        order.MarkAsPaid();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
