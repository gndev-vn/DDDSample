using Shared.Common;
using Shared.Messaging.Order;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderPaidDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; }
}

public class OrderPaidDomainEventHandler
{
    public static async Task HandleAsync(OrderPaidDomainEvent @event, IMessageBus bus,
        ILogger<OrderPaidDomainEventHandler> logger)
    {
        logger.LogInformation("Order {Id} paid.", @event.Id);
        await bus.PublishAsync(new OrderPaidEvent
        {
            Id = @event.Id,
            Total = @event.Total,
            Currency = @event.Currency
        });
    }
}