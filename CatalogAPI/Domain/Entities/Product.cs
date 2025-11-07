using CatalogAPI.Domain.Events;
using Shared.Common;
using Shared.ValueObjects;

namespace CatalogAPI.Domain.Entities;

public sealed class Product : EntityWithEvents
{
    private readonly List<ProductVariant> _variants = [];

    public Product()
    {
        // EF Core requires default constructor
    }

    public Product(string name, string description, string slug, Money basePrice, bool isActive = true)
    {
        GuardNotNullOrWhiteSpace(slug, nameof(slug));
        GuardNotNullOrWhiteSpace(name, nameof(name));

        BasePrice = basePrice ?? throw new ArgumentNullException(nameof(basePrice));
        Slug = slug.Trim();
        Name = name.Trim();
        Description = description ?? string.Empty;
        IsActive = isActive;
        AddDomainEvent(new ProductCreatedDomainEvent
        {
            Id = Id,
            Name = Name,
            Currency = BasePrice.Currency,
            Slug = Slug,
            CurrentPrice = BasePrice.Amount,
            ImageUrl = ImageUrl
        });
    }

    public Money BasePrice { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;

    public IReadOnlyList<ProductVariant> Variants => _variants;
    public Category? Category { get; set; }

    public ProductVariant AddVariant(string name, string sku, string description, Money? overridePrice = null,
        IEnumerable<VariantAttribute>? attributes = null)
    {
        GuardNotNullOrWhiteSpace(name, nameof(name));
        GuardNotNullOrWhiteSpace(sku, nameof(sku));

        var variant = new ProductVariant(name, sku, description, overridePrice, attributes);
        _variants.Add(variant);
        return variant;
    }

    public bool TryGetVariantBySku(string sku, out ProductVariant? variant)
    {
        variant = null;
        if (string.IsNullOrWhiteSpace(sku))
        {
            return false;
        }

        variant = _variants.FirstOrDefault(v => string.Equals(v.Sku, sku.Trim(), StringComparison.OrdinalIgnoreCase));
        return variant is not null;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        Name = newName.Trim();
    }

    public void Reprice(Money newBasePrice)
    {
        BasePrice = newBasePrice ?? throw new ArgumentNullException(nameof(newBasePrice));
    }

    public void SetImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl?.Trim() ?? string.Empty;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public static Product Create(string name, string description, string slug, Money basePrice, bool isActive = true)
        => new(name, description, slug, basePrice, isActive);

    public void UpdateDetails(string productName, string productDescription, string productSlug, Money productBasePrice,
        bool isActive = true)
    {
        GuardNotNullOrWhiteSpace(productName, nameof(productName));
        GuardNotNullOrWhiteSpace(productSlug, nameof(productSlug));

        Name = productName.Trim();
        Description = productDescription;
        Slug = productSlug.Trim();
        BasePrice = productBasePrice ?? throw new ArgumentNullException(nameof(productBasePrice));
        IsActive = isActive;
        AddDomainEvent(new ProductUpdatedDomainEvent
        {
            Id = Id
        });
    }

    private static void GuardNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} required", paramName);
        }
    }
}