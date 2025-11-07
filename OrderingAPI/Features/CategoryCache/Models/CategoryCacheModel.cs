namespace OrderingAPI.Features.CategoryCache.Models;

public class CategoryCacheModel
{
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }
}