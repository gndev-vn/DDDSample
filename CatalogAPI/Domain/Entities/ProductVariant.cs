using Shared.Models;
using Shared.ValueObjects;

namespace CatalogAPI.Domain.Entities;

public sealed class ProductVariant : Entity
{
    private readonly List<VariantAttribute> _attributes = [];

    public ProductVariant()
    {
    }

    public ProductVariant(
        string name,
        string sku,
        string description,
        Money? overridePrice = null,
        IEnumerable<VariantAttribute>? attributes = null)
    {
        GuardNotNullOrWhiteSpace(sku, nameof(sku));
        GuardNotNullOrWhiteSpace(name, nameof(name));

        Name = name.Trim();
        Sku = sku.Trim();
        Description = description ?? string.Empty;
        OverridePrice = overridePrice;

        ReplaceAttributes(attributes);
    }

    public string Name { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Money? OverridePrice { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<VariantAttribute> Attributes => _attributes;

    public Product? Product { get; set; }

    public Guid ProductId { get; set; }

    public void ReplaceAttributes(IEnumerable<VariantAttribute>? attributes)
    {
        _attributes.Clear();

        if (attributes is null)
        {
            return;
        }

        foreach (var attribute in attributes)
        {
            AddOrReplaceAttribute(attribute);
        }
    }

    public void AddOrReplaceAttribute(VariantAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        var index = _attributes.FindIndex(a => a.AttributeId == attribute.AttributeId);
        if (index >= 0)
        {
            _attributes[index] = attribute;
        }
        else
        {
            _attributes.Add(attribute);
        }
    }

    public void UpdateDetails(
        string name,
        string sku,
        string description,
        Money? overridePrice,
        IEnumerable<VariantAttribute>? attributes)
    {
        GuardNotNullOrWhiteSpace(name, nameof(name));
        GuardNotNullOrWhiteSpace(sku, nameof(sku));

        Name = name.Trim();
        Sku = sku.Trim();
        Description = description ?? string.Empty;
        OverridePrice = overridePrice;
        ReplaceAttributes(attributes);
    }

    public void SetOverridePrice(Money? newPrice) => OverridePrice = newPrice;

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        Name = newName.Trim();
    }

    public void SetDescription(string description) => Description = description ?? string.Empty;

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
        }
    }

    public static ProductVariant Create(
        string name,
        string sku,
        string description,
        Money? overridePrice,
        IEnumerable<VariantAttribute>? attributes)
        => new(name, sku, description, overridePrice, attributes);

    private static void GuardNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} required", paramName);
        }
    }
}
