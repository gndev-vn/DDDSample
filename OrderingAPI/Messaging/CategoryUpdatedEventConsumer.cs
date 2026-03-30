using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class CategoryUpdatedEventConsumer
{
    public static async Task HandleAsync(CategoryUpdatedEvent @event, ILogger<CategoryUpdatedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Consumed category updated event {Id}", @event.Id);
        var cache = await dbContext.CategoryCaches.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);
        if (cache is null)
        {
            await dbContext.CategoryCaches.AddAsync(new CategoryCache
            {
                Id = @event.Id,
                Name = @event.Name,
                ParentId = @event.ParentId,
                IsActive = @event.IsActive
            }, cancellationToken);
        }
        else
        {
            cache.Name = @event.Name;
            cache.ParentId = @event.ParentId;
            cache.IsActive = @event.IsActive;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
