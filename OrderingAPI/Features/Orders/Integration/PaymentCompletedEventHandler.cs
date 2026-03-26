using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Enums;
using Shared.Messaging.Payment;

namespace OrderingAPI.Features.Orders.Integration;

public class PaymentCompletedEventHandler
{
    public static async Task HandleAsync(PaymentCompletedEvent @event, AppDbContext dbContext,
        ILogger<PaymentCompletedEventHandler> logger, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.SingleOrDefaultAsync(x => x.Id == @event.OrderId, cancellationToken);
        if (order == null)
        {
            logger.LogWarning("Order {OrderId} was not found for completed payment {PaymentId}", @event.OrderId,
                @event.PaymentId);
            return;
        }

        if (order.Status == OrderStatus.Paid)
        {
            logger.LogInformation("Order {OrderId} is already marked as paid; ignoring duplicate payment event {PaymentId}",
                @event.OrderId, @event.PaymentId);
            return;
        }

        order.Pay();
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Order {OrderId} marked as paid from payment event {PaymentId}", @event.OrderId,
            @event.PaymentId);
    }
}
