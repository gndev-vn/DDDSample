namespace CatalogAPI.Features.Products.CreateProduct;

public record ProductCreateRequest(
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    decimal BasePrice,
    string Currency);

