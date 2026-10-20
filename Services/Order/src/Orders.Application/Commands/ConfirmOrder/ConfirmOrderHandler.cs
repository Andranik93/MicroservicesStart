using MediatR;
using Orders.Application.Abstractions;
using Orders.Application.Common.Exceptions;
using Orders.Application.IntegrationEvents;
using Orders.Domain.OrderAggregate;

namespace Orders.Application.Commands.ConfirmOrder;

public sealed class ConfirmOrderHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher) : IRequestHandler<ConfirmOrderCommand>
{
    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Confirm();

        // Save FIRST, then publish - the opposite order from the synchronous
        // HTTP version this replaces, and deliberately so:
        //
        // With the HTTP call, "call Payment first, save second" let a failure
        // cleanly undo itself - nothing persisted, nothing elsewhere happened,
        // the order just stayed Pending and the request could be retried as-is.
        //
        // A message, once published, is durably on the broker and WILL likely
        // be processed - it can't be "rolled back" the way skipping a save can.
        // So publishing first and having the save fail afterward is the worse
        // bug: Payment proceeds to open a payment for an order that, as far as
        // Order's own database is concerned, was never actually confirmed.
        //
        // Saving first avoids that specific bug, but trades it for a smaller
        // one: if the publish below fails, the order IS correctly Confirmed,
        // Payment just never learns about it. That gap - a local write that
        // succeeds without the event that should accompany it - is exactly
        // what the Outbox Pattern closes, by writing the event in the SAME
        // database transaction as the entity change. Not built yet; a named,
        // deliberate simplification for this step.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var @event = new OrderConfirmedEvent(order.Id, order.Total.Amount, order.Currency, DateTimeOffset.UtcNow);
        await eventPublisher.PublishOrderConfirmedAsync(@event, cancellationToken);
    }
}
