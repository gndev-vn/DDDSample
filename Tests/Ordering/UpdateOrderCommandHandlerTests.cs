using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.GetOrderById;
using OrderingAPI.Features.Orders.UpdateOrder;
using Shared.Enums;
using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace DDDSample.Tests.Ordering;

public sealed class UpdateOrderCommandHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public UpdateOrderCommandHandlerTests()
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

    private AppDbContext NewCtx() => new AppDbContext(_options);

    private async Task<Guid> SeedOrderAsync(List<OrderLine>? lines = null, Address? shippingAddress = null, Address? billingAddress = null)
    {
        var order = Order.Create(
            Guid.NewGuid(),
            "Alex Nguyen",
            "alex@example.com",
            "+84 901 000 111",
            lines ?? [new OrderLine(new Sku("SKU-001"), Quantity.Of(2), new Money(10m, "USD"))],
            shippingAddress ?? OldShipping(),
            billingAddress);

        await using var ctx = NewCtx();
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        return order.Id;
    }

    private static Address OldShipping() => new("123 Old Street", null, "Old Ward", "Old District", "Old City", "Old Province");

    private static OrderModel BuildModel(Guid orderId, Guid customerId, AddressModel? shippingAddress = null, List<OrderLineModel>? lines = null) =>
        new()
        {
            Id = orderId,
            CustomerId = customerId,
            ShippingAddress = shippingAddress ?? new AddressModel("456 New Road", null, "New City", "New Province", "New District", "New Ward"),
            Lines = lines ?? [new OrderLineModel { Sku = "SKU-NEW", Quantity = 5, UnitPrice = 20m, Currency = "USD" }]
        };

    [Fact]
    public async Task Handle_WhenOrderExists_ReplacesLinesAndShippingAddress()
    {
        var orderId = await SeedOrderAsync(lines: [new OrderLine(new Sku("SKU-1"), Quantity.Of(1), new Money(10m, "USD"))]);

        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId,
            shippingAddress: new AddressModel("456 New Road", null, "New City", "New Province", "New District", "New Ward"),
            lines: [new OrderLineModel { Sku = "SKU-2", Quantity = 3, UnitPrice = 25m, Currency = "USD" }]));

        var result = await handler.Handle(command, CancellationToken.None);

        await using var assertCtx = NewCtx();
        var updated = await assertCtx.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == orderId);
        Assert.Equal("456 New Road", updated.ShippingAddress!.Line1);
        Assert.Single(updated.Lines);
        Assert.Equal("SKU-2", updated.Lines.Single().Sku.Value);
        Assert.Equal(3, updated.Lines.Single().Quantity.Value);
        Assert.Equal("SKU-2", result.Lines.Single().Sku);
    }

    [Fact]
    public async Task Handle_WithMultipleNewLines_ReplacesAllOldLines()
    {
        var orderId = await SeedOrderAsync(lines:
        [
            new OrderLine(new Sku("OLD-A"), Quantity.Of(1), new Money(10m, "USD")),
            new OrderLine(new Sku("OLD-B"), Quantity.Of(2), new Money(20m, "USD"))
        ]);

        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId,
            lines:
            [
                new OrderLineModel { Sku = "NEW-X", Quantity = 1, UnitPrice = 5m, Currency = "USD" },
                new OrderLineModel { Sku = "NEW-Y", Quantity = 4, UnitPrice = 15m, Currency = "USD" },
                new OrderLineModel { Sku = "NEW-Z", Quantity = 2, UnitPrice = 8m, Currency = "USD" }
            ]));

        await handler.Handle(command, CancellationToken.None);

        await using var assertCtx = NewCtx();
        var updated = await assertCtx.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == orderId);
        Assert.Equal(3, updated.Lines.Count);
        Assert.DoesNotContain(updated.Lines, l => l.Sku.Value is "OLD-A" or "OLD-B");
        var skus = updated.Lines.Select(l => l.Sku.Value).ToHashSet();
        Assert.Contains("NEW-X", skus);
        Assert.Contains("NEW-Y", skus);
        Assert.Contains("NEW-Z", skus);
    }

    [Fact]
    public async Task Handle_UpdatesAllShippingAddressFields()
    {
        var orderId = await SeedOrderAsync();
        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        var newAddress = new AddressModel("789 Boulevard", "Apt 4B", "Metropolis", "Metro Province", "Central District", "Ward 7");

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId, shippingAddress: newAddress));

        await handler.Handle(command, CancellationToken.None);

        await using var assertCtx = NewCtx();
        var addr = (await assertCtx.Orders.FindAsync(orderId))!.ShippingAddress!;
        Assert.Equal("789 Boulevard", addr.Line1);
        Assert.Equal("Apt 4B", addr.Line2);
        Assert.Equal("Metropolis", addr.City);
        Assert.Equal("Metro Province", addr.Province);
        Assert.Equal("Central District", addr.District);
        Assert.Equal("Ward 7", addr.Ward);
    }

    [Fact]
    public async Task Handle_ReturnedModelReflectsUpdatedState()
    {
        var orderId = await SeedOrderAsync();
        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId,
            shippingAddress: new AddressModel("Return Check Rd", null, "RC City", "RC Province", "RC District", "RC Ward"),
            lines: [new OrderLineModel { Sku = "RC-SKU", Quantity = 7, UnitPrice = 12m, Currency = "USD" }]));

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(orderId, result.Id);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Return Check Rd", result.ShippingAddress!.Line1);
        Assert.Equal("RC City", result.ShippingAddress.City);
        Assert.Equal(OrderStatus.Submitted, result.Status);
        Assert.Single(result.Lines);
        Assert.Equal("RC-SKU", result.Lines[0].Sku);
        Assert.Equal(7, result.Lines[0].Quantity);
        Assert.Equal(12m, result.Lines[0].UnitPrice);
        Assert.Equal("USD", result.Lines[0].Currency);
    }

    [Fact]
    public async Task Handle_PreservesBillingAddressAndCustomerId()
    {
        var billing = new Address("1 Billing Lane", null, "Bill Ward", "Bill District", "Bill City", "Bill Province");
        var orderId = await SeedOrderAsync(billingAddress: billing);

        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId));

        await handler.Handle(command, CancellationToken.None);

        await using var assertCtx = NewCtx();
        var updated = await assertCtx.Orders.FindAsync(orderId);
        Assert.Equal(customerId, updated!.CustomerId);
        Assert.Equal("1 Billing Lane", updated.BillingAddress?.Line1);
        Assert.Equal("Bill City", updated.BillingAddress?.City);
    }

    [Fact]
    public async Task Handle_StatusRemainsUnchanged_AfterUpdate()
    {
        var orderId = await SeedOrderAsync();
        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);

        var result = await handler.Handle(new UpdateOrderCommand(BuildModel(orderId, customerId)), CancellationToken.None);

        Assert.Equal(OrderStatus.Submitted, result.Status);
    }

    [Fact]
    public async Task Handle_WhenCommandHasDuplicateSkus_MergesQuantitiesPerDomainRule()
    {
        var orderId = await SeedOrderAsync();
        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId,
            lines:
            [
                new OrderLineModel { Sku = "DUPE", Quantity = 3, UnitPrice = 10m, Currency = "USD" },
                new OrderLineModel { Sku = "DUPE", Quantity = 4, UnitPrice = 10m, Currency = "USD" }
            ]));

        await handler.Handle(command, CancellationToken.None);

        await using var assertCtx = NewCtx();
        var updated = await assertCtx.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == orderId);
        Assert.Single(updated.Lines);
        Assert.Equal(7, updated.Lines.Single().Quantity.Value);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(Guid.NewGuid(), Guid.NewGuid()));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WhenShippingAddressIsNull_ThrowsArgumentException()
    {
        var orderId = await SeedOrderAsync();
        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(new OrderModel
        {
            Id = orderId,
            CustomerId = customerId,
            ShippingAddress = null,
            Lines = [new OrderLineModel { Sku = "X", Quantity = 1, UnitPrice = 1m, Currency = "USD" }]
        });

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_WhenOrderIsPaid_ThrowsDomainException()
    {
        var orderId = await SeedOrderAsync();
        await using (var ctx = NewCtx())
        {
            var order = await ctx.Orders.FindAsync(orderId);
            order!.Pay();
            await ctx.SaveChangesAsync();
        }

        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId));

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None).AsTask());
        Assert.Contains("modified", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WhenOrderIsCancelled_ThrowsDomainException()
    {
        var orderId = await SeedOrderAsync();
        await using (var ctx = NewCtx())
        {
            var order = await ctx.Orders.FindAsync(orderId);
            order!.Cancel();
            await ctx.SaveChangesAsync();
        }

        Guid customerId;
        await using (var ctx = NewCtx())
            customerId = (await ctx.Orders.FindAsync(orderId))!.CustomerId;

        await using var handlerCtx = NewCtx();
        var handler = new UpdateOrderCommandHandler(handlerCtx);
        var command = new UpdateOrderCommand(BuildModel(orderId, customerId));

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None).AsTask());
        Assert.Contains("modified", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

