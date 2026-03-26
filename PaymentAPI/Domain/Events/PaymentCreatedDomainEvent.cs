using Shared.Models;
using Wolverine;

namespace PaymentAPI.Domain.Events;

public class PaymentCreatedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class PaymentCreatedDomainEventHandler
{
    public static async Task HandleAsync(PaymentCreatedDomainEvent @event, IMessageBus bus,
        ILogger<PaymentCreatedDomainEventHandler> logger)
    {
        logger.LogInformation("Payment {PaymentId} created for order {OrderId}", @event.PaymentId, @event.OrderId);
        await bus.PublishAsync(new Shared.Messaging.Payment.PaymentCreatedEvent
        {
            PaymentId = @event.PaymentId,
            OrderId = @event.OrderId,
            Amount = @event.Amount,
            Currency = @event.Currency
        });
    }
}
