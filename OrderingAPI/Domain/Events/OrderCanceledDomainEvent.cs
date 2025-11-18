using Shared.Messaging.Order;
using Shared.Models;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderCanceledDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
}

public class OrderCanceledDomainEventHandler
{
    public static async Task HandleAsync(OrderCanceledDomainEvent @event, IMessageBus bus,
        ILogger<OrderCanceledDomainEventHandler> logger)
    {
        logger.LogInformation("Order {Id} canceled.", @event.Id);
        await bus.PublishAsync(new OrderCanceledEvent
        {
            Id = @event.Id
        });
    }
}