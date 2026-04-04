using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Messaging.Payment;
using Wolverine.Attributes;

namespace OrderingAPI.Features.Orders.Integration;

[ScheduleRetry(typeof(DbUpdateException), 5, 15, 30)]
[RetryNow(typeof(TimeoutException), 50, 100, 250)]
public class PaymentFailedEventHandler
{
    public static async Task HandleAsync(PaymentFailedEvent @event, AppDbContext dbContext,
        ILogger<PaymentFailedEventHandler> logger, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "[OrderingAPI] Consuming PaymentFailedEvent for order {OrderId} and payment {PaymentId}: {Reason}",
            @event.OrderId,
            @event.PaymentId,
            @event.Reason);

        var orderExists = await dbContext.Orders
            .AsNoTracking()
            .AnyAsync(x => x.Id == @event.OrderId, cancellationToken);

        if (!orderExists)
        {
            logger.LogWarning(
                "[OrderingAPI] Order {OrderId} was not found for PaymentFailedEvent {PaymentId}",
                @event.OrderId,
                @event.PaymentId);
            return;
        }

        logger.LogInformation(
            "[OrderingAPI] Order {OrderId} remains unpaid after PaymentFailedEvent {PaymentId}",
            @event.OrderId,
            @event.PaymentId);
    }
}
