namespace CatalogAPI.Features.Products.CreateProductVariant;

public sealed record ProductVariantAttributeValueRequest(Guid AttributeId, string Value);

public sealed record ProductVariantCreateRequest(
    string Name,
    string Sku,
    string Description,
    Guid ParentId,
    decimal OverridePrice,
    string Currency,
    List<ProductVariantAttributeValueRequest> Attributes);
