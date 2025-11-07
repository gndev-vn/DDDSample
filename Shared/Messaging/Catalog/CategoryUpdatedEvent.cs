using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.category.updated")]
public class CategoryUpdatedEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
}