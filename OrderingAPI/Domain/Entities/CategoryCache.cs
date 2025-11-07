using Shared.Common;

namespace OrderingAPI.Domain.Entities;

public class CategoryCache : Entity
{
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentId { get; set; }
}