using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public class CategoryDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}

public class CategoryDeletedDomainEventHandler
{
    public static async Task HandleAsync(CategoryDeletedDomainEvent @event, IMessageBus bus,
        ILogger<CategoryDeletedDomainEventHandler> logger)
    {
        logger.LogInformation("Category {Id} deleted", @event.Id);
        await bus.PublishAsync(new CategoryDeletedEvent { Id = @event.Id });
    }
}