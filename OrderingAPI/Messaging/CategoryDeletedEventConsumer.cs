using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Messaging.Catalog;

namespace OrderingAPI.Messaging;

public class CategoryDeletedEventConsumer
{
    public static async Task HandleAsync(CategoryDeletedEvent @event, ILogger<CategoryDeletedEventConsumer> logger,
        AppDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Consumed category deleted event {Id}", @event.Id);
        var cache = await dbContext.CategoryCaches.FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);
        if (cache is null)
        {
            return;
        }

        dbContext.CategoryCaches.Remove(cache);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
