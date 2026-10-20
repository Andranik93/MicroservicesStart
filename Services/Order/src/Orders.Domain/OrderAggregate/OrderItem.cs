using Orders.Domain.ValueObjects;

namespace Orders.Domain.OrderAggregate;

/// <summary>
/// Line item owned by the <see cref="Order"/> aggregate. Only the aggregate
/// root creates and changes it, so validation happens there.
/// </summary>
public sealed class OrderItem
{
    public Guid ProductId { get; }
    public Money UnitPrice { get; } = null!;
    public int Quantity { get; private set; }

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    internal OrderItem(Guid productId, Money unitPrice, int quantity)
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    internal void IncreaseQuantity(int by) => Quantity += by;

    // EF Core materializes a blank instance through this constructor, then
    // writes ProductId/UnitPrice/Quantity straight to their backing fields,
    // since none of them has a public or internal setter. Never called by
    // domain code - the "= null!" above only silences the nullable warning
    // for this path.
    private OrderItem()
    {
    }
}
