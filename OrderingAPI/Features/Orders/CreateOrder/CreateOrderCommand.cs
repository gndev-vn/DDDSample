using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Features.Orders.CreateOrder;

public record CreateOrderCommand(Guid CustomerId, List<OrderLineModel> Lines, AddressModel ShippingAddress, AddressModel? BillingAddress) : IRequest<OrderModel>;

public class CreateOrderCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateOrderCommand, OrderModel>
{
    public async ValueTask<OrderModel> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var normalizedSkuMap = await ResolveProductCacheMapAsync(command.Lines, cancellationToken);
        var newOrder = Order.Create(
            command.CustomerId,
            command.Lines.Select(line => ToOrderLine(line, normalizedSkuMap)).ToList(),
            command.ShippingAddress.Adapt<Address>(),
            command.BillingAddress?.Adapt<Address>());

        await dbContext.Orders.AddAsync(newOrder, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToOrderModel(newOrder, normalizedSkuMap);
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

    private static OrderModel ToOrderModel(Order order, IReadOnlyDictionary<string, ProductCache> productsBySku)
    {
        return new OrderModel
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
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