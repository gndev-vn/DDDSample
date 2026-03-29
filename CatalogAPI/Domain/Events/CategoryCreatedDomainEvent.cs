using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class CategoryCreatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid ParentId { get; set; }
}

public sealed class CategoryCreatedDomainEventHandler
{
    public static async Task HandleAsync(CategoryCreatedDomainEvent @event, IMessageBus bus,
        ILogger<CategoryCreatedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Publishing CategoryCreatedEvent for category {CategoryId} ({CategoryName})", @event.Id, @event.Name);
        await bus.PublishAsync(new CategoryCreatedEvent
        {
            Id = @event.Id,
            ParentId = @event.ParentId,
            Name = @event.Name
        });
    }
}