using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Exceptions;

namespace OrderingAPI.Features.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(string DisplayName, string Email, string? PhoneNumber, bool IsActive, string? Address = null) : IRequest<CustomerModel>;

public sealed class CreateCustomerCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateCustomerCommand, CustomerModel>
{
    public async ValueTask<CustomerModel> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.Trim();
        var exists = await dbContext.Customers.AnyAsync(customer => customer.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException($"Customer email '{normalizedEmail}' is already in use.");
        }

        var customer = Customer.Create(command.DisplayName, normalizedEmail, command.PhoneNumber, command.IsActive, command.Address);
        await dbContext.Customers.AddAsync(customer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CustomerModel
        {
            Id = customer.Id,
            DisplayName = customer.DisplayName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            IsActive = customer.IsActive,
            OrderCount = 0,
        };
    }
}
