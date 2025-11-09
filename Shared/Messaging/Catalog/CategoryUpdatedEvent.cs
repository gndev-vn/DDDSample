using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.category.updated")]
public class CategoryUpdatedEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; } = string.Empty;
}