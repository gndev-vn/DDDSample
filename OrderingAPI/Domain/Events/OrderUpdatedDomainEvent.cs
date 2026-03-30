using Shared.Messaging.Order;
using Shared.Models;
using Wolverine;

namespace OrderingAPI.Domain.Events;

public class OrderUpdatedDomainEvent : DomainEvent
{
    public Guid Id { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class OrderUpdatedDomainEventHandler
{
    public static async Task HandleAsync(OrderUpdatedDomainEvent @event, IMessageBus bus, ILogger logger)
    {
        logger.LogInformation("[OrderingAPI] Publishing OrderUpdatedEvent for order {OrderId} with total {Total} {Currency}",
            @event.Id, @event.Total, @event.Currency);
        await bus.PublishAsync(new OrderUpdatedEvent
        {
            Id = @event.Id,
            ShippingAddress = @event.ShippingAddress,
            BillingAddress = @event.BillingAddress
        });
    }
}
