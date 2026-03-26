using Shared.Models;
using Wolverine;

namespace PaymentAPI.Domain.Events;

public class PaymentFailedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class PaymentFailedDomainEventHandler
{
    public static async Task HandleAsync(PaymentFailedDomainEvent @event, IMessageBus bus,
        ILogger<PaymentFailedDomainEventHandler> logger)
    {
        logger.LogWarning("Payment {PaymentId} failed for order {OrderId}: {Reason}", @event.PaymentId, @event.OrderId,
            @event.Reason);
        await bus.PublishAsync(new Shared.Messaging.Payment.PaymentFailedEvent
        {
            PaymentId = @event.PaymentId,
            OrderId = @event.OrderId,
            Amount = @event.Amount,
            Currency = @event.Currency,
            Reason = @event.Reason
        });
    }
}
