using Payments.Domain.Exceptions;

namespace Payments.Domain.ValueObjects;

/// <summary>
/// Immutable amount of money in a single currency. Equality is by value.
///
/// Deliberately duplicated from the Order service's Money - each service owns
/// its own domain model and there is no shared library between services.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new DomainException("Currency must be a 3-letter ISO code.");

        return new Money(decimal.Round(amount, 2), currency.ToUpperInvariant());
    }
}
