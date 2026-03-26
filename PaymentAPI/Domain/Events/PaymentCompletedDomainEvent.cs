using Shared.Models;
using Wolverine;

namespace PaymentAPI.Domain.Events;

public class PaymentCompletedDomainEvent : DomainEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
}

public class PaymentCompletedDomainEventHandler
{
    public static async Task HandleAsync(PaymentCompletedDomainEvent @event, IMessageBus bus,
        ILogger<PaymentCompletedDomainEventHandler> logger)
    {
        logger.LogInformation("Payment {PaymentId} completed for order {OrderId}", @event.PaymentId, @event.OrderId);
        await bus.PublishAsync(new Shared.Messaging.Payment.PaymentCompletedEvent
        {
            PaymentId = @event.PaymentId,
            OrderId = @event.OrderId,
            Amount = @event.Amount,
            Currency = @event.Currency,
            TransactionReference = @event.TransactionReference
        });
    }
}
