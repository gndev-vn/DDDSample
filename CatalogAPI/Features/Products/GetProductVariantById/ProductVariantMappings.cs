using System.Linq.Expressions;
using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Features.Products.GetProductVariantById;

public static class ProductVariantMappings
{
    public static readonly Expression<Func<ProductVariant, ProductVariantResponse>> Projection = x =>
        new ProductVariantResponse
        {
            Id = x.Id,
            ParentId = x.ProductId,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            OverridePrice = x.OverridePrice != null ? x.OverridePrice.Amount : null,
            Currency = x.OverridePrice != null ? x.OverridePrice.Currency : x.Product.BasePrice.Currency,
            IsActive = x.IsActive,
            Attributes = x.Attributes
                .Select(attribute => new ProductVariantAttributeResponse
                {
                    AttributeId = attribute.AttributeId,
                    Name = attribute.Name,
                    Value = attribute.Value,
                })
                .ToList(),
        };

    public static ProductVariantResponse ToResponse(ProductVariant variant, string productCurrency)
    {
        return new ProductVariantResponse
        {
            Id = variant.Id,
            ParentId = variant.ProductId,
            Name = variant.Name,
            Sku = variant.Sku,
            Description = variant.Description,
            OverridePrice = variant.OverridePrice?.Amount,
            Currency = variant.OverridePrice?.Currency ?? productCurrency,
            IsActive = variant.IsActive,
            Attributes = variant.Attributes
                .Select(attribute => new ProductVariantAttributeResponse
                {
                    AttributeId = attribute.AttributeId,
                    Name = attribute.Name,
                    Value = attribute.Value,
                })
                .ToList(),
        };
    }
}
