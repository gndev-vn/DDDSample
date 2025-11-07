using Shared.Common;
using Shared.Messaging.Catalog;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public class CategoryCreatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public Guid ParentId { get; set; }
}

public class CategoryCreatedDomainEventHandler
{
    public static async Task HandleAsync(CategoryCreatedDomainEvent @event, IMessageBus bus,
        ILogger<CategoryCreatedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Handling CategoryCreatedDomainEvent: {@Event}", @event);
        await bus.PublishAsync(new CategoryCreatedEvent
        {
            Id = @event.Id,
            ParentId = @event.ParentId,
            Name = @event.Name
        });
    }
}