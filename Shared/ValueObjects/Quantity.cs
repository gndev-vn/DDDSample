using Shared.Common;
using Shared.Exceptions;

namespace Shared.ValueObjects;

public class Quantity(int value) : ValueObject
{
    public int Value { get; } = value;

    public static Quantity Of(int value, int min = 1, int? max = null, int step = 1)
    {
        if (value < min)
        {
            throw new DomainException($"Quantity must be ≥ {min}");
        }

        if (max is not null && value > max)
        {
            throw new DomainException($"Quantity must be ≤ {max}");
        }

        return value % step != 0
            ? throw new DomainException($"Quantity must be multiple of {step}")
            : new Quantity(value);
    }

    public static Quantity Zero() => new(0);
    public bool IsZero => Value == 0;

    public static Quantity operator +(Quantity a, int other) => new(a.Value + other);

    public static Quantity operator +(Quantity a, Quantity other) => new(a.Value + other.Value);

    public static Quantity operator -(Quantity a, int other) => new(a.Value - other);

    public static Quantity operator -(Quantity a, Quantity other) => new(a.Value - other.Value);

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}