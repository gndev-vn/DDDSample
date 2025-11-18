namespace CatalogAPI.Features.Categories.Models;

public record CategoryCreateRequest(string Name, string Slug, string Description, Guid ParentId);