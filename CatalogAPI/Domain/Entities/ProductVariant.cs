using Shared.Models;
using Shared.ValueObjects;

namespace CatalogAPI.Domain.Entities;

public sealed class ProductVariant : Entity
{
    private readonly List<VariantAttribute> _attributes = [];

    public ProductVariant()
    {
        // EF Core requires default constructor
    }

    public ProductVariant(string name, string sku, string description, Money? overridePrice = null,
        IEnumerable<VariantAttribute>? attributes = null)
    {
        GuardNotNullOrWhiteSpace(sku, nameof(sku));
        GuardNotNullOrWhiteSpace(name, nameof(name));

        Name = name.Trim();
        Sku = sku.Trim();
        Description = description ?? string.Empty;
        OverridePrice = overridePrice;

        if (attributes is null)
        {
            return;
        }

        foreach (var attr in attributes)
        {
            AddOrReplaceAttribute(attr);
        }
    }

    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money? OverridePrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<VariantAttribute> Attributes => _attributes;
    public Product? Product { get; set; }
    public Guid ProductId { get; set; }

    public void AddOrReplaceAttribute(VariantAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        var index = _attributes.FindIndex(a => string.Equals(a.Name, attribute.Name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            _attributes[index] = attribute;
        }
        else
        {
            _attributes.Add(attribute);
        }
    }

    public bool RemoveAttributeByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var idx = _attributes.FindIndex(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
        {
            return false;
        }

        _attributes.RemoveAt(idx);
        return true;
    }

    public void UpdateDetails(string name, string sku, string description, Money? overridePrice)
    {
        GuardNotNullOrWhiteSpace(name, nameof(name));
        GuardNotNullOrWhiteSpace(sku, nameof(sku));

        Name = name.Trim();
        Sku = sku.Trim();
        Description = description ?? string.Empty;
        OverridePrice = overridePrice;
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

    public static ProductVariant Create(string name, string sku, string description, Money? overridePrice,
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
