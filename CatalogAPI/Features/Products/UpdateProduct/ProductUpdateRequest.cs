namespace CatalogAPI.Features.Products.UpdateProduct;

public record ProductUpdateRequest(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    decimal BasePrice,
    string Currency);

