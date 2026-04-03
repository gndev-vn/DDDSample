using CatalogAPI.Domain.Events;
using Shared.Models;
using Shared.ValueObjects;

namespace CatalogAPI.Domain.Entities;

public sealed class Product : EntityWithEvents
{
    private readonly List<ProductVariant> _variants = [];

    public Product()
    {
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
            Sku = Slug,
            Name = Name,
            Currency = BasePrice.Currency,
            Slug = Slug,
            CurrentPrice = BasePrice.Amount,
            ImageUrl = ImageUrl,
            IsActive = IsActive,
        });
    }

    public Money? BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public IReadOnlyList<ProductVariant> Variants => _variants;

    public Category? Category { get; set; }

    public ProductVariant AddVariant(
        string name,
        string sku,
        string description,
        Money? overridePrice = null,
        IEnumerable<VariantAttribute>? attributes = null)
    {
        GuardNotNullOrWhiteSpace(name, nameof(name));
        GuardNotNullOrWhiteSpace(sku, nameof(sku));

        if (TryGetVariantBySku(sku, out _))
        {
            throw new InvalidOperationException($"Variant sku '{sku}' already exists for product '{Id}'.");
        }

        var variant = new ProductVariant(name, sku, description, overridePrice, attributes)
        {
            Product = this,
            ProductId = Id,
        };

        _variants.Add(variant);
        var price = ResolveVariantPrice(variant);
        AddDomainEvent(new ProductVariantCreatedDomainEvent
        {
            Id = variant.Id,
            ProductId = Id,
            Sku = variant.Sku,
            Name = variant.Name,
            CurrentPrice = price.Amount,
            Currency = price.Currency,
            IsActive = variant.IsActive,
        });

        return variant;
    }

    public ProductVariant UpdateVariant(
        Guid variantId,
        string name,
        string sku,
        string description,
        Money? overridePrice = null,
        IEnumerable<VariantAttribute>? attributes = null)
    {
        var variant = GetVariantById(variantId);
        var normalizedSku = sku.Trim();

        if (!string.Equals(variant.Sku, normalizedSku, StringComparison.OrdinalIgnoreCase)
            && TryGetVariantBySku(normalizedSku, out _))
        {
            throw new InvalidOperationException($"Variant sku '{sku}' already exists for product '{Id}'.");
        }

        variant.UpdateDetails(name, normalizedSku, description, overridePrice, attributes);
        var price = ResolveVariantPrice(variant);
        AddDomainEvent(new ProductVariantUpdatedDomainEvent
        {
            Id = variant.Id,
            ProductId = Id,
            Sku = variant.Sku,
            Name = variant.Name,
            CurrentPrice = price.Amount,
            Currency = price.Currency,
            IsActive = variant.IsActive,
        });

        return variant;
    }

    public ProductVariant RemoveVariant(Guid variantId)
    {
        var variant = GetVariantById(variantId);
        _variants.Remove(variant);

        AddDomainEvent(new ProductVariantDeletedDomainEvent
        {
            Id = variant.Id,
            ProductId = Id,
            Sku = variant.Sku,
        });

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

    public void MarkDeleted() => AddDomainEvent(new ProductDeletedDomainEvent { Id = Id });

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
            Id = Id,
            Name = Name,
            CurrentPrice = BasePrice.Amount,
            Currency = BasePrice.Currency,
            Slug = Slug,
            ImageUrl = ImageUrl,
            IsActive = IsActive,
        });
    }

    private ProductVariant GetVariantById(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        return variant ?? throw new KeyNotFoundException("Invalid product variant id");
    }

    private Money ResolveVariantPrice(ProductVariant variant)
    {
        if (variant.OverridePrice is not null)
        {
            return variant.OverridePrice;
        }

        return BasePrice ?? throw new InvalidOperationException("Product base price must be set before managing variants.");
    }

    private static void GuardNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} required", paramName);
        }
    }
}
