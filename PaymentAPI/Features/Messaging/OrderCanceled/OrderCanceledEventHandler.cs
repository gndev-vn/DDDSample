using Microsoft.EntityFrameworkCore;
using PaymentAPI.Domain;
using Shared.Messaging.Order;
using Wolverine.Attributes;

namespace PaymentAPI.Features.Messaging.OrderCanceled;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class OrderCanceledEventHandler
{
    public static async Task HandleAsync(OrderCanceledEvent @event, AppDbContext dbContext,
        ILogger<OrderCanceledEventHandler> logger, CancellationToken cancellationToken)
    {
        logger.LogInformation("[PaymentAPI] Consuming OrderCanceledEvent for order {OrderId}", @event.Id);

        var payment = await dbContext.Payments.FirstOrDefaultAsync(x => x.OrderId == @event.Id, cancellationToken);
        if (payment is null)
        {
            logger.LogInformation(
                "[PaymentAPI] No payment exists for order {OrderId}; skipping OrderCanceledEvent",
                @event.Id);
            return;
        }

        if (payment.Status != Domain.Enums.PaymentStatus.Pending)
        {
            logger.LogInformation(
                "[PaymentAPI] Payment {PaymentId} for order {OrderId} is {Status}; skipping cancellation handling",
                payment.Id,
                @event.Id,
                payment.Status);
            return;
        }

        payment.Fail("Order was cancelled before payment completion.");
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "[PaymentAPI] Marked payment {PaymentId} as failed because order {OrderId} was cancelled",
            payment.Id,
            @event.Id);
    }
}
