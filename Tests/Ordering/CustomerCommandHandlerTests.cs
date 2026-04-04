using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Customers.CreateCustomer;
using OrderingAPI.Features.Customers.DeleteCustomer;
using OrderingAPI.Features.Customers.UpdateCustomer;
using Shared.Exceptions;

namespace DDDSample.Tests.Ordering;

public sealed class CustomerCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public CustomerCommandHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var ctx = new AppDbContext(_options);
        ctx.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext NewContext() => new(_options);

    [Fact]
    public async Task CreateCustomer_PersistsCustomerRecord()
    {
        await using var dbContext = NewContext();
        var handler = new CreateCustomerCommandHandler(dbContext);

        var result = await handler.Handle(new CreateCustomerCommand("Alex Nguyen", "alex@example.com", "+84 901 000 111", true), CancellationToken.None);

        await using var assertContext = NewContext();
        var customer = await assertContext.Customers.SingleAsync(item => item.Id == result.Id);
        Assert.Equal("Alex Nguyen", customer.DisplayName);
        Assert.Equal("alex@example.com", customer.Email);
        Assert.Equal("+84 901 000 111", customer.PhoneNumber);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public async Task UpdateCustomer_ChangesEditableFields()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Jamie Tran", "jamie@example.com", null, true);
        await using (var seedContext = NewContext())
        {
            seedContext.Customers.Add(customer);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new UpdateCustomerCommandHandler(dbContext);

        var result = await handler.Handle(new UpdateCustomerCommand(customer.Id, "Jamie T.", "jamie.t@example.com", "+84 902 000 222", false), CancellationToken.None);

        Assert.Equal("Jamie T.", result.DisplayName);
        Assert.Equal("jamie.t@example.com", result.Email);
        Assert.Equal("+84 902 000 222", result.PhoneNumber);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task DeleteCustomer_WhenOrdersReferenceCustomer_ThrowsBusinessRuleException()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Order Customer", "order.customer@example.com", null, true);
        var order = Order.Create(
            customer.Id,
            customer.DisplayName,
            customer.Email,
            customer.PhoneNumber,
            [new OrderLine(new Shared.ValueObjects.Sku("SKU-1"), Shared.ValueObjects.Quantity.Of(1), new Shared.ValueObjects.Money(10m, "USD"))],
            new Shared.ValueObjects.Address("123 Main", null, "Ward 1", "District 1", "HCMC", "HCM"));

        await using (var seedContext = NewContext())
        {
            seedContext.Customers.Add(customer);
            seedContext.Orders.Add(order);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = NewContext();
        var handler = new DeleteCustomerCommandHandler(dbContext);

        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new DeleteCustomerCommand(customer.Id), CancellationToken.None).AsTask());
    }
}
