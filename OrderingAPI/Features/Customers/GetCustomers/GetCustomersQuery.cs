using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;

namespace OrderingAPI.Features.Customers.GetCustomers;

public sealed record GetCustomersQuery(string? Search = null) : IRequest<IReadOnlyList<CustomerModel>>;

public sealed class GetCustomersQueryHandler(AppDbContext dbContext) : IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerModel>>
{
    public async ValueTask<IReadOnlyList<CustomerModel>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Customers.AsNoTracking();
        var search = request.Search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(customer =>
                customer.DisplayName.Contains(search) ||
                customer.Email.Contains(search) ||
                (customer.PhoneNumber != null && customer.PhoneNumber.Contains(search)) ||
                (customer.Address != null && customer.Address.Contains(search)));
        }

        return await query
            .OrderBy(customer => customer.DisplayName)
            .Select(customer => new CustomerModel
            {
                Id = customer.Id,
                DisplayName = customer.DisplayName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                IsActive = customer.IsActive,
                OrderCount = dbContext.Orders.Count(order => order.CustomerId == customer.Id),
            })
            .ToListAsync(cancellationToken);
    }
}
