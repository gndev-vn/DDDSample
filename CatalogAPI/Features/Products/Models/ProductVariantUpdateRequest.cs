namespace CatalogAPI.Features.Products.Models;

public record ProductVariantUpdateRequest(
    Guid Id,
    string Name,
    string Sku,
    string Description,
    Guid ParentId,
    decimal OverridePrice,
    string Currency);