using Shared.Common;

namespace Shared.ValueObjects;

public class Sku : ValueObject
{
    private bool Equals(Sku other)
    {
        return base.Equals(other) && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((Sku)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Value);
    }

    public string Value { get; }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 64)
        {
            throw new ArgumentException("SKU must be non-empty and ≤ 64 chars.");
        }

        Value = value.ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static bool operator ==(Sku a, Sku other) =>
        string.Compare(a.Value, other.Value, StringComparison.InvariantCultureIgnoreCase) == 0;

    public static bool operator !=(Sku a, Sku other) =>
        string.Compare(a.Value, other.Value, StringComparison.InvariantCultureIgnoreCase) != 0;

    public override string ToString() => Value;
}