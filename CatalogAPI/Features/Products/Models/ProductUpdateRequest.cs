using Shared.Models;

namespace CatalogAPI.Features.Products.Models;

public record ProductUpdateRequest(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid CategoryId,
    decimal BasePrice,
    string Currency);