using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using Shared.Exceptions;

namespace OrderingAPI.Features.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(Guid Id, string DisplayName, string Email, string? PhoneNumber, bool IsActive) : IRequest<CustomerModel>;

public sealed class UpdateCustomerCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateCustomerCommand, CustomerModel>
{
    public async ValueTask<CustomerModel> Handle(UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.FirstOrDefaultAsync(item => item.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Customer", command.Id);

        var normalizedEmail = command.Email.Trim();
        var duplicateEmail = await dbContext.Customers.AnyAsync(item => item.Id != command.Id && item.Email == normalizedEmail, cancellationToken);
        if (duplicateEmail)
        {
            throw new BusinessRuleException($"Customer email '{normalizedEmail}' is already in use.");
        }

        customer.Update(command.DisplayName, normalizedEmail, command.PhoneNumber, command.IsActive);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CustomerModel
        {
            Id = customer.Id,
            DisplayName = customer.DisplayName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            IsActive = customer.IsActive,
            OrderCount = await dbContext.Orders.CountAsync(order => order.CustomerId == customer.Id, cancellationToken)
        };
    }
}
