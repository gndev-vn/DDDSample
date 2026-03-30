using Mapster;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class CategoryCreatedEventConsumer
{
    // Wolverine will pick this up by convention
    public static async Task HandleAsync(
        CategoryCreatedEvent @event,
        ILogger<CategoryCreatedEventConsumer> logger,
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("[OrderingAPI] Consuming CategoryCreatedEvent for category {CategoryId} ({CategoryName})",
                @event.Id, @event.Name);
            var category = @event.Adapt<CategoryCache>();
            if (await dbContext.CategoryCaches.AnyAsync(x => x.Id == category.Id, cancellationToken))
            {
                logger.LogInformation("[OrderingAPI] Category cache already exists for category {CategoryId}; skipping CategoryCreatedEvent",
                    @event.Id);
                return;
            }

            await dbContext.CategoryCaches.AddAsync(category, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[OrderingAPI] Error consuming CategoryCreatedEvent: {@Event}", @event);
            throw;
        }
    }
}
