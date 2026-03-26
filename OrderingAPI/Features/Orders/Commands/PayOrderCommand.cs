using Mediator;
using OrderingAPI.Domain;
using Shared.Enums;
using Shared.Exceptions;

namespace OrderingAPI.Features.Orders.Commands;

public record PayOrderCommand(Guid Id) : IRequest<bool>;

public class PayOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<PayOrderCommand, bool>
{
    public async ValueTask<bool> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync(request.Id,
            cancellationToken: cancellationToken);
        if (order == null)
        {
            throw new NotFoundException("Order not found");
        }

        order.Pay();
        dbContext.Update(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
