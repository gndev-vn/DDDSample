using Shared.Messaging.Catalog;
using Shared.Models;
using Wolverine;

namespace CatalogAPI.Domain.Events;

public sealed class CategoryUpdatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class CategoryUpdatedDomainEventHandler
{
    public async Task HandleAsync(CategoryUpdatedDomainEvent @event, IMessageBus bus,
        ILogger<CategoryUpdatedDomainEventHandler> logger)
    {
        logger.LogInformation("[CatalogAPI] Publishing CategoryUpdatedEvent for category {CategoryId}", @event.Id);
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