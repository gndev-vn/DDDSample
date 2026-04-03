using Shared.Models;

namespace Shared.ValueObjects;

public sealed class VariantAttribute : ValueObject
{
    public VariantAttribute()
    {
    }

    public VariantAttribute(Guid attributeId, string name, string value)
    {
        if (attributeId == Guid.Empty)
        {
            throw new ArgumentException("attributeId required", nameof(attributeId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("name required", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("value required", nameof(value));
        }

        AttributeId = attributeId;
        Name = name.Trim();
        Value = value.Trim();
    }

    public Guid AttributeId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return AttributeId;
        yield return Name.ToLowerInvariant();
        yield return Value.ToLowerInvariant();
    }
}
