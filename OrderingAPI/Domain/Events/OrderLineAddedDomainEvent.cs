using Shared.Messaging.Order;
using Shared.Models;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderLineAddedDomainEvent : DomainEvent
{
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}

public class OrderLineAddedDomainEventHandler
{
    public static async Task HandleAsync(OrderLineAddedDomainEvent @event, IMessageBus bus, ILogger logger)
    {
        logger.LogInformation("[OrderingAPI] Publishing OrderLineAddedEvent for order {OrderId}, line {OrderLineId}, quantity {Quantity}",
            @event.OrderId, @event.OrderLineId, @event.Quantity);
        await bus.PublishAsync(new OrderLineAddedEvent
        {
            OrderId = @event.OrderId,
            OrderLineId = @event.OrderLineId,
            Quantity = @event.Quantity,
            ProductId = @event.ProductId,
            Total = @event.Total
        });
    }
}
