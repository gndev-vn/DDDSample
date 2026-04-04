namespace CatalogAPI.Features.Products.UpdateProductVariant;

public sealed record ProductVariantAttributeValueRequest(Guid AttributeId, string Value);

public sealed record ProductVariantUpdateRequest(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    Guid ParentId,
    decimal? OverridePrice,
    string Currency,
    List<ProductVariantAttributeValueRequest> Attributes);
