using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.Commands;
using OrderingAPI.Features.Orders.Models;
using Shared.Models;
using Shared.ValueObjects;

namespace DDDSample.Tests.Ordering;

public sealed class UpdateOrderCommandHandlerTests
{
    [Fact]
    public async Task UpdateOrder_ReplacesPersistedLinesAndShippingAddress()
    {
        await using var dbContext = CreateDbContext();
        var order = Order.Create(
            Guid.NewGuid(),
            [new OrderLine(new Sku("SKU-1"), Quantity.Of(1), new Money(10m, "USD"))],
            new Address("Old line", null, "Old ward", "Old district", "Old city", "Old province"));
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateOrderCommandHandler(dbContext);
        var command = new UpdateOrderCommand(new OrderModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            ShippingAddress = new AddressModel("New line", null, "New city", "New province", "New district", "New ward"),
            Lines =
            [
                new OrderLineModel
                {
                    Sku = "SKU-2",
                    Quantity = 3,
                    UnitPrice = 25m,
                    Currency = "USD"
                }
            ]
        });

        var result = await handler.Handle(command, CancellationToken.None);
        var updatedOrder = await dbContext.Orders.Include(x => x.Lines).SingleAsync(x => x.Id == order.Id);

        Assert.Equal("New line", updatedOrder.ShippingAddress?.Line1);
        Assert.Single(updatedOrder.Lines);
        Assert.Equal("SKU-2", updatedOrder.Lines.Single().Sku.Value);
        Assert.Equal("SKU-2", result.Lines.Single().Sku);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
