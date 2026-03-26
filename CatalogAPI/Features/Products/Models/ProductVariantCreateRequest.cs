using Shared.ValueObjects;

namespace CatalogAPI.Features.Products.Models;

public record ProductVariantCreateRequest(
    string Name,
    string Sku,
    string Description,
    Guid ParentId,
    decimal OverridePrice,
    string Currency,
    List<VariantAttribute> Attributes);