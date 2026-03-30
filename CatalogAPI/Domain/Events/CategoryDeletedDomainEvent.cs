using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class CategoryDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}

public sealed class CategoryDeletedDomainEventHandler
{
    public static async Task HandleAsync(CategoryDeletedDomainEvent @event, IMessageBus bus,
        ILogger<CategoryDeletedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Publishing CategoryDeletedEvent for category {CategoryId}", @event.Id);
        await bus.PublishAsync(new CategoryDeletedEvent { Id = @event.Id });
    }
}