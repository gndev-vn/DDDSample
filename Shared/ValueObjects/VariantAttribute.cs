using Shared.Models;

namespace Shared.ValueObjects;

public sealed class VariantAttribute : ValueObject
{
    public VariantAttribute()
    {
    }

    public VariantAttribute(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name required");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("value required");
        }

        Name = name;
        Value = value;
    }

    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
        yield return Value.ToLowerInvariant();
    }
}
