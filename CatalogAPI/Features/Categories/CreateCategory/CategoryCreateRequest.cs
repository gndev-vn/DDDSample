namespace CatalogAPI.Features.Categories.CreateCategory;

public record CategoryCreateRequest(string Name, string Slug, string Description, Guid? ParentId);

