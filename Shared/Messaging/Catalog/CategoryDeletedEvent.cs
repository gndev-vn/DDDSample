using Wolverine.Attributes;

namespace Shared.Messaging.Catalog;

[Topic("catalog.category.deleted")]
public class CategoryDeletedEvent
{
    public Guid Id { get; set; }
}