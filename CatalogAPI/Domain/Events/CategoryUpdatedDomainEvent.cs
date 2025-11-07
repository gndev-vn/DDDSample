using Shared.Common;
using Shared.Messaging.Catalog;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public class CategoryUpdatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; }
}

public class CategoryUpdatedDomainEventHandler
{
    public async Task HandleAsync(CategoryUpdatedDomainEvent @event, IMessageBus bus,
        ILogger<CategoryUpdatedDomainEventHandler> logger)
    {
        logger.LogInformation("Category updated: {@event}", @event);
        await bus.PublishAsync(new CategoryUpdatedEvent
        {
            Id = @event.Id,
            Name = @event.Name,
            Slug = @event.Slug,
            ParentId = @event.ParentId,
            IsActive = @event.IsActive,
            Description = @event.Description
        });
    }
}