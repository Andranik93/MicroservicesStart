using Orders.Domain.Exceptions;
using Orders.Domain.OrderAggregate;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Tests.OrderAggregate;

public class OrderTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();

    private static Order CreatePendingOrder() => Order.Create(CustomerId, "USD");

    private static Money Usd(decimal amount) => Money.Create(amount, "USD");

    [Fact]
    public void Create_NewOrder_IsPendingWithZeroTotal()
    {
        var order = CreatePendingOrder();

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.Total.Amount);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        var act = () => Order.Create(Guid.Empty, "USD");

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void AddItem_CalculatesTotalFromLineItems()
    {
        var order = CreatePendingOrder();

        order.AddItem(ProductId, Usd(10m), 3);
        order.AddItem(Guid.NewGuid(), Usd(2.5m), 2);

        Assert.Equal(35m, order.Total.Amount);
    }

    [Fact]
    public void AddItem_SameProductTwice_IncreasesQuantityInsteadOfAddingLine()
    {
        var order = CreatePendingOrder();

        order.AddItem(ProductId, Usd(10m), 2);
        order.AddItem(ProductId, Usd(10m), 3);

        var item = Assert.Single(order.Items);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void AddItem_SameProductAtDifferentPrice_Throws()
    {
        var order = CreatePendingOrder();
        order.AddItem(ProductId, Usd(10m), 1);

        var act = () => order.AddItem(ProductId, Usd(12m), 1);

        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantity_Throws(int quantity)
    {
        var order = CreatePendingOrder();

        var act = () => order.AddItem(ProductId, Usd(10m), quantity);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void AddItem_WithDifferentCurrency_Throws()
    {
        var order = CreatePendingOrder();

        var act = () => order.AddItem(ProductId, Money.Create(10m, "EUR"), 1);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Confirm_PendingOrderWithItems_BecomesConfirmed()
    {
        var order = CreatePendingOrder();
        order.AddItem(ProductId, Usd(10m), 1);

        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_OrderWithNoItems_Throws()
    {
        var order = CreatePendingOrder();

        var act = () => order.Confirm();

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Confirm_AlreadyConfirmedOrder_Throws()
    {
        var order = CreatePendingOrder();
        order.AddItem(ProductId, Usd(10m), 1);
        order.Confirm();

        var act = () => order.Confirm();

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void AddItem_AfterConfirm_Throws()
    {
        var order = CreatePendingOrder();
        order.AddItem(ProductId, Usd(10m), 1);
        order.Confirm();

        var act = () => order.AddItem(Guid.NewGuid(), Usd(5m), 1);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Cancel_PendingOrder_BecomesCancelled()
    {
        var order = CreatePendingOrder();

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_ConfirmedOrder_Throws()
    {
        var order = CreatePendingOrder();
        order.AddItem(ProductId, Usd(10m), 1);
        order.Confirm();

        var act = () => order.Cancel();

        Assert.Throws<DomainException>(act);
    }
}
