using Shared.Models;

namespace CatalogAPI.Features.Categories.Models;

public class CategoryModel : ModelBase
{
    public bool IsActive { get; set; } = true;

    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}