using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public class ProductDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}

public class ProductDeletedDomainEventHandler
{
    public static async Task HandleAsync(ProductDeletedDomainEvent @event, IMessageBus bus,
        ILogger<ProductDeletedDomainEventHandler> logger)
    {
        logger.LogInformation("Product {Id} created", @event.Id);
        await bus.PublishAsync(new ProductDeletedEvent { Id = @event.Id });
    }
}