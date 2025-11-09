using Shared.Messaging.Order;
using Shared.Models;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderDeletedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}

public class OrderDeletedDomainEventHandler
{
    public static async Task HandleAsync(OrderDeletedDomainEvent @event, IMessageBus bus, ILogger logger)
    {
        logger.LogInformation("Order deleted {Id}", @event.Id);
        await bus.PublishAsync(new OrderDeletedEvent
        {
            Id = @event.Id
        });
    }
}