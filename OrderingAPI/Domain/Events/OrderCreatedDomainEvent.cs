using Shared.Messaging.Order;
using Shared.Models;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderCreatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string BillingAddress { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
}

public class OrderCreatedDomainEventHandler
{
    public static async Task HandleAsync(OrderCreatedDomainEvent @event, IMessageBus bus,
        ILogger<OrderCreatedDomainEventHandler> logger)
    {
        logger.LogInformation("[OrderingAPI] Publishing OrderCreatedEvent for order {OrderId} with total {Total} {Currency}",
            @event.Id, @event.Total, @event.Currency);
        await bus.PublishAsync(new OrderCreatedEvent
        {
            Id = @event.Id,
            Total = @event.Total,
            Currency = @event.Currency,
            ShippingAddress = @event.ShippingAddress,
            BillingAddress = @event.BillingAddress
        });
    }
}