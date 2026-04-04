using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Features.Orders.UpdateOrder;

public record UpdateOrderCommand(OrderModel Model) : IRequest<OrderModel>;

public class UpdateOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateOrderCommand, OrderModel>
{
    public async ValueTask<OrderModel> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await LoadOrderAsync(command.Model.Id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException();
        }

        Customer? customer = null;
        if (order.CustomerId != command.Model.CustomerId)
        {
            customer = await ResolveCustomerAsync(command.Model.CustomerId, cancellationToken);
        }

        var shippingAddress = command.Model.ShippingAddress?.Adapt<Address>() ?? throw new ArgumentException("Shipping address is required");
        var normalizedSkuMap = await ResolveProductCacheMapAsync(command.Model.Lines, cancellationToken);
        var lines = command.Model.Lines.Select(line => ToOrderLine(line, normalizedSkuMap)).ToList();
        var existingLines = order.Lines.ToList();

        if (existingLines.Count != 0)
        {
            dbContext.OrderLines.RemoveRange(existingLines);
            await dbContext.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            order = await LoadOrderAsync(command.Model.Id, cancellationToken);
            if (order == null)
            {
                throw new KeyNotFoundException();
            }
        }

        if (customer is not null)
        {
            order.ChangeCustomer(customer.Id, customer.DisplayName, customer.Email, customer.PhoneNumber);
        }

        order.Update(shippingAddress, lines);
        dbContext.Entry(order).State = EntityState.Modified;
        foreach (var line in order.Lines)
        {
            dbContext.Entry(line).State = EntityState.Added;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderModel(order, normalizedSkuMap);
    }

    private async Task<Customer> ResolveCustomerAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == customerId, cancellationToken)
            ?? throw new NotFoundException("Customer", customerId);

        if (!customer.IsActive)
        {
            throw new BusinessRuleException($"Customer '{customer.DisplayName}' is inactive.");
        }

        return customer;
    }

    private async Task<Dictionary<string, ProductCache>> ResolveProductCacheMapAsync(IEnumerable<OrderLineModel> lines, CancellationToken cancellationToken)
    {
        var requestedSkus = lines
            .Select(line => new Sku(line.Sku).Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var products = await dbContext.ProductCaches
            .Where(product => requestedSkus.Contains(product.Sku))
            .ToListAsync(cancellationToken);

        return products.ToDictionary(product => product.Sku, StringComparer.OrdinalIgnoreCase);
    }

    private static OrderLine ToOrderLine(OrderLineModel model, IReadOnlyDictionary<string, ProductCache> productsBySku)
    {
        var sku = new Sku(model.Sku);
        var orderLine = new OrderLine(sku, Quantity.Of(model.Quantity), ResolveUnitPrice(model, productsBySku, sku.Value));

        if (productsBySku.TryGetValue(sku.Value, out var cachedProduct))
        {
            orderLine.ProductId = cachedProduct.Id;
        }

        return orderLine;
    }

    private static Money ResolveUnitPrice(OrderLineModel model, IReadOnlyDictionary<string, ProductCache> productsBySku, string normalizedSku)
    {
        if (productsBySku.TryGetValue(normalizedSku, out var cachedProduct))
        {
            if (!cachedProduct.IsActive)
            {
                throw new BusinessRuleException($"Product with sku '{normalizedSku}' is inactive.");
            }

            return new Money(cachedProduct.CurrentPrice, cachedProduct.Currency);
        }

        return new Money(model.UnitPrice, model.Currency);
    }

    private async Task<Order?> LoadOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    private static OrderModel ToOrderModel(Order order, IReadOnlyDictionary<string, ProductCache> productsBySku)
    {
        return new OrderModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress == null ? null : new AddressModel(
                order.ShippingAddress.Line1,
                order.ShippingAddress.Line2,
                order.ShippingAddress.City,
                order.ShippingAddress.Province,
                order.ShippingAddress.District,
                order.ShippingAddress.Ward),
            Lines = order.Lines.Select(line =>
            {
                productsBySku.TryGetValue(line.Sku.Value, out var cachedProduct);
                return new OrderLineModel
                {
                    Id = line.Id,
                    ProductId = line.ProductId,
                    Name = cachedProduct?.Name ?? string.Empty,
                    Sku = line.Sku.Value,
                    Quantity = line.Quantity.Value,
                    UnitPrice = line.Total.Amount,
                    Currency = line.Total.Currency
                };
            }).ToList()
        };
    }
}
