using Shared.Models;

namespace CatalogAPI.Features.Products.Models;

public record ProductCreateRequest(
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    decimal BasePrice,
    string Currency);