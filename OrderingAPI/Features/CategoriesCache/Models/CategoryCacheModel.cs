namespace OrderingAPI.Features.CategoriesCache.Models;

public class CategoryCacheModel
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }
}