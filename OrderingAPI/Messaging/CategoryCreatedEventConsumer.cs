using Mapster;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Database;
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
            logger.LogInformation("[OrderingAPI] Successfully processed CategoryCreatedEvent for category {Name} ({Id})",
                @event.Name, @event.Id);
            var category = @event.Adapt<CategoryCache>();
            if (await dbContext.CategoryCaches.AnyAsync(x => x.Id == category.Id, cancellationToken))
            {
                logger.LogInformation("Category already exists {Id}", @event.Id);
                throw new InvalidOperationException("Category already exists");
            }

            await dbContext.CategoryCaches.AddAsync(category, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[OrderingAPI] Error processing CategoryCreatedEvent: {@Event}", @event);
            throw;
        }
    }
}