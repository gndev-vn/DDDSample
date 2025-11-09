using Shared.Models;

namespace Shared.ValueObjects;

public sealed class Money : ValueObject
{
    public Money()
    {
        // EF Core requires default constructor    
    }

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("currency required");
        }

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public decimal Amount { get; }

    public string Currency { get; } = string.Empty;

    public override string ToString() => $"{Amount} {Currency}";

    public static Money Zero(string currency) => new Money(0, currency);

    public static Money operator +(Money a, Money b)
    {
        return a.Currency != b.Currency
            ? throw new InvalidOperationException($"Cannot add {a} and {b}")
            : new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator +(Money a, decimal? b)
    {
        return new Money(a.Amount + b ?? 0, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        return a.Currency != b.Currency
            ? throw new InvalidOperationException($"Cannot add {a} and {b}")
            : new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal b)
    {
        return new Money(a.Amount * b, a.Currency);
    }

    public static Money operator /(Money a, decimal b)
    {
        return new Money(a.Amount / b, a.Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency.ToLowerInvariant();
    }
}