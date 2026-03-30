using System.Linq.Expressions;
using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Features.Products.Models;

public static class ProductVariantMappings
{
    public static readonly Expression<Func<ProductVariant, ProductVariantResponse>> Projection = x =>
        new ProductVariantResponse
        {
            Id = x.Id,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            OverridePrice = x.OverridePrice != null ? x.OverridePrice.Amount : 0,
            Currency = x.OverridePrice != null ? x.OverridePrice.Currency : string.Empty,
            IsActive = x.IsActive,
            Attributes = x.Attributes.ToList()
        };

    public static ProductVariantResponse ToResponse(ProductVariant variant)
    {
        return new ProductVariantResponse
        {
            Id = variant.Id,
            Name = variant.Name,
            Sku = variant.Sku,
            Description = variant.Description,
            OverridePrice = variant.OverridePrice?.Amount ?? 0,
            Currency = variant.OverridePrice?.Currency ?? string.Empty,
            IsActive = variant.IsActive,
            Attributes = variant.Attributes.ToList()
        };
    }
}
