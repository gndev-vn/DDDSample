using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Messaging.Payment;
using Wolverine.Attributes;
using OrderingAPI.Domain;

namespace OrderingAPI.Features.Orders.Integration;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class PaymentCompletedEventHandler
{
    public static async Task HandleAsync(PaymentCompletedEvent @event, AppDbContext dbContext,
        ILogger<PaymentCompletedEventHandler> logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("[OrderingAPI] Consuming PaymentCompletedEvent for order {OrderId} and payment {PaymentId}", @event.OrderId, @event.PaymentId);

        var order = await dbContext.Orders.SingleOrDefaultAsync(x => x.Id == @event.OrderId, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("[OrderingAPI] Order {OrderId} was not found for PaymentCompletedEvent from payment {PaymentId}", @event.OrderId,
                @event.PaymentId);
            return;
        }

        if (order.Status == OrderStatus.Paid)
        {
            logger.LogInformation("[OrderingAPI] Order {OrderId} is already marked as paid; skipping duplicate PaymentCompletedEvent from payment {PaymentId}",
                @event.OrderId, @event.PaymentId);
            return;
        }

        order.Pay();
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("[OrderingAPI] Marked order {OrderId} as paid from PaymentCompletedEvent {PaymentId}", @event.OrderId,
            @event.PaymentId);
    }
}

