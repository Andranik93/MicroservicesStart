using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.OrderAggregate;

/// <summary>
/// Aggregate root. All changes to an order go through this class so its
/// business rules always hold.
/// </summary>
public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public string Currency { get; }
    public OrderStatus Status { get; private set; }
    public string? PaymentFailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Money Total => _items.Aggregate(
        Money.Zero(Currency),
        (sum, item) => sum.Add(item.LineTotal));

    private Order(Guid id, Guid customerId, string currency, DateTimeOffset createdAtUtc)
    {
        Id = id;
        CustomerId = customerId;
        Currency = Money.Zero(currency).Currency;
        Status = OrderStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public static Order Create(Guid customerId, string currency)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");

        return new Order(Guid.NewGuid(), customerId, currency, DateTimeOffset.UtcNow);
    }

    public void AddItem(Guid productId, Money unitPrice, int quantity)
    {
        EnsurePending();

        if (productId == Guid.Empty)
            throw new DomainException("ProductId is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice.Currency != Currency)
            throw new DomainException(
                $"Item currency {unitPrice.Currency} does not match order currency {Currency}.");

        var existing = _items.SingleOrDefault(i => i.ProductId == productId);

        if (existing is null)
        {
            _items.Add(new OrderItem(productId, unitPrice, quantity));
            return;
        }

        if (existing.UnitPrice != unitPrice)
            throw new DomainException(
                $"Product {productId} is already in the order at a different price.");

        existing.IncreaseQuantity(quantity);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Only pending orders can be confirmed. Current status: {Status}.");

        if (_items.Count == 0)
            throw new DomainException("Cannot confirm an order with no items.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Only pending orders can be cancelled. Current status: {Status}.");

        Status = OrderStatus.Cancelled;
    }

    // "Confirmed" only ever meant "payment was requested," not "payment
    // succeeded" - these two are the real outcome, reported back by Payment.
    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException($"Only a confirmed order can be marked paid. Current status: {Status}.");

        Status = OrderStatus.Paid;
    }

    public void MarkPaymentFailed(string reason)
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException($"Only a confirmed order can have a failed payment. Current status: {Status}.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A failure reason is required.");

        Status = OrderStatus.PaymentFailed;
        PaymentFailureReason = reason;
    }

    private void EnsurePending()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Items can only be changed on a pending order. Current status: {Status}.");
    }
}
