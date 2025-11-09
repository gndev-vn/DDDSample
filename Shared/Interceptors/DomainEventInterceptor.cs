using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.Models;
using Wolverine;

namespace Shared.Interceptors;

public class DomainEventInterceptor(IMessageBus messageBus, string localEventsQueue, ILogger<DomainEventInterceptor> logger)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
        {
            return result;
        }

        var events = context.ChangeTracker.Entries<IEntityWithEvents>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // Clear domain events before saving to avoid re-publishing
        foreach (var entry in context.ChangeTracker.Entries<IEntityWithEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }

        // Store the original SaveChanges result
        var saveResult = await base.SavingChangesAsync(eventData, result, cancellationToken);

        // Publish events after successful save
        if (events.Count == 0)
        {
            return saveResult;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var eventName = eventType.Name;

                logger.LogInformation("Publishing domain event: {EventName} {@Event}", eventName, @event);
                await messageBus.EndpointFor(localEventsQueue).SendAsync(@event);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing domain events");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return saveResult;
    }
}