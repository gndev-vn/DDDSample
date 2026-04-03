namespace CatalogAPI.Features.Categories.UpdateCategory;

public record CategoryUpdateRequest(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    Guid ParentId,
    bool? IsActive = true);

