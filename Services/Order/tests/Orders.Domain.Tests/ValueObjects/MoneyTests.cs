using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        var act = () => Money.Create(-1m, "USD");

        Assert.Throws<DomainException>(act);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDX")]
    [InlineData("")]
    public void Create_WithInvalidCurrency_Throws(string currency)
    {
        var act = () => Money.Create(10m, currency);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Create_NormalizesCurrencyToUppercase()
    {
        var money = Money.Create(10m, "usd");

        Assert.Equal("USD", money.Currency);
    }

    [Theory]
    [InlineData(10.005, 10.00)]
    [InlineData(10.015, 10.02)]
    [InlineData(10.5, 10.50)]
    public void Create_RoundsToTwoDecimalPlaces(decimal input, decimal expected)
    {
        var money = Money.Create(input, "USD");

        Assert.Equal(expected, money.Amount);
    }

    [Fact]
    public void Add_SameCurrency_SumsAmounts()
    {
        var total = Money.Create(10m, "USD").Add(Money.Create(5.5m, "USD"));

        Assert.Equal(15.5m, total.Amount);
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        var usd = Money.Create(10m, "USD");
        var eur = Money.Create(10m, "EUR");

        var act = () => usd.Add(eur);

        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var first = Money.Create(10m, "USD");
        var second = Money.Create(10m, "USD");

        Assert.Equal(first, second);
    }
}
