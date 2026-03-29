using Shared.Messaging.Order;
using Shared.Models;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderPaidDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class OrderPaidDomainEventHandler
{
    public static async Task HandleAsync(OrderPaidDomainEvent @event, IMessageBus bus,
        ILogger<OrderPaidDomainEventHandler> logger)
    {
        logger.LogInformation("[OrderingAPI] Publishing OrderPaidEvent for order {OrderId}", @event.Id);
        await bus.PublishAsync(new OrderPaidEvent
        {
            Id = @event.Id,
            Total = @event.Total,
            Currency = @event.Currency
        });
    }
}