using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Exceptions;

namespace OrderingAPI.Features.Customers.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteCustomerCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteCustomerCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Customer", command.Id);

        var hasOrders = await dbContext.Orders.AnyAsync(order => order.CustomerId == command.Id, cancellationToken);
        if (hasOrders)
        {
            throw new BusinessRuleException("Customer cannot be deleted because existing orders reference it.");
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
