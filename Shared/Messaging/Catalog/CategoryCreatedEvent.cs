using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.category.created")]
public class CategoryCreatedEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid ParentId { get; set; }
    public string ParentName { get; set; }
}