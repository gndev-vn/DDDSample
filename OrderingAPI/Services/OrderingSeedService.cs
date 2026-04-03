using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderingAPI.Configuration;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using Shared.Enums;
using Shared.ValueObjects;

namespace OrderingAPI.Services;

public sealed class OrderingSeedService(
    AppDbContext dbContext,
    IOptions<OrderingSeedOptions> seedOptions,
    ILogger<OrderingSeedService> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!seedOptions.Value.Enabled)
        {
            logger.LogInformation("[OrderingAPI] Ordering seed is disabled.");
            return;
        }

        await EnsureProductCachesAsync(cancellationToken);
        await EnsureOrdersAsync(cancellationToken);

        logger.LogInformation("[OrderingAPI] Ordering seed completed.");
    }

    private async Task EnsureProductCachesAsync(CancellationToken cancellationToken)
    {
        var productCachesBySku = await dbContext.ProductCaches
            .ToDictionaryAsync(cache => cache.Sku, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var seed in ProductCacheSeeds)
        {
            if (productCachesBySku.TryGetValue(seed.Sku, out var existing))
            {
                if (existing.Name == seed.Name &&
                    existing.CurrentPrice == seed.CurrentPrice &&
                    string.Equals(existing.Currency, seed.Currency, StringComparison.OrdinalIgnoreCase) &&
                    existing.IsActive == seed.IsActive &&
                    string.Equals(existing.ImageUrl, seed.ImageUrl, StringComparison.Ordinal))
                {
                    logger.LogInformation("[OrderingAPI] Seed product cache {Sku} already exists.", seed.Sku);
                    continue;
                }

                existing.Name = seed.Name;
                existing.CurrentPrice = seed.CurrentPrice;
                existing.Currency = seed.Currency;
                existing.ImageUrl = seed.ImageUrl;
                existing.IsActive = seed.IsActive;
                existing.LastUpdatedUtc = now;
                existing.UpdatedAtUtc = now;
                logger.LogInformation("[OrderingAPI] Refreshed seed product cache {Sku}.", seed.Sku);
                continue;
            }

            await dbContext.ProductCaches.AddAsync(new ProductCache
            {
                Id = seed.Id,
                Sku = seed.Sku,
                Name = seed.Name,
                CurrentPrice = seed.CurrentPrice,
                Currency = seed.Currency,
                ImageUrl = seed.ImageUrl,
                IsActive = seed.IsActive,
                LastUpdatedUtc = now,
                UpdatedAtUtc = now
            }, cancellationToken);

            logger.LogInformation("[OrderingAPI] Seeded product cache {Sku}.", seed.Sku);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureOrdersAsync(CancellationToken cancellationToken)
    {
        var existingCustomerIds = await dbContext.Orders
            .Select(order => order.CustomerId)
            .ToListAsync(cancellationToken);

        var existingCustomerIdSet = existingCustomerIds.ToHashSet();

        foreach (var seed in OrderSeeds)
        {
            if (existingCustomerIdSet.Contains(seed.CustomerId))
            {
                logger.LogInformation("[OrderingAPI] Seed order for customer {CustomerId} already exists.", seed.CustomerId);
                continue;
            }

            var lines = seed.Lines.Select(line => new OrderLine(
                new Sku(line.Sku),
                Quantity.Of(line.Quantity),
                new Money(line.UnitPrice, line.Currency))
            {
                ProductId = line.ProductId
            }).ToList();

            var order = Order.Create(
                seed.CustomerId,
                lines,
                new Address(
                    seed.ShippingAddress.Line1,
                    seed.ShippingAddress.Line2,
                    seed.ShippingAddress.Ward,
                    seed.ShippingAddress.District,
                    seed.ShippingAddress.City,
                    seed.ShippingAddress.Province),
                seed.BillingAddress is null
                    ? null
                    : new Address(
                        seed.BillingAddress.Line1,
                        seed.BillingAddress.Line2,
                        seed.BillingAddress.Ward,
                        seed.BillingAddress.District,
                        seed.BillingAddress.City,
                        seed.BillingAddress.Province));

            if (seed.Status == OrderStatus.Paid)
            {
                order.Pay();
            }

            order.ClearDomainEvents();
            await dbContext.Orders.AddAsync(order, cancellationToken);
            logger.LogInformation("[OrderingAPI] Seeded {Status} order for customer {CustomerId}.", seed.Status, seed.CustomerId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static readonly OrderingProductCacheSeed[] ProductCacheSeeds =
    [
        new(Guid.Parse("1f4b328d-15bf-4dd2-9a06-7b44f78f2d01"), "DDD-HOODIE-BLK-M", "DDD Hoodie / Black / M", 59.90m, "USD", "https://images.example.local/catalog/ddd-hoodie.png", true),
        new(Guid.Parse("e22c58c3-4e8b-4623-90a6-a5ecb1308902"), "ASPIRE-MUG-WHT-12OZ", "Aspire Mug / White / 12oz", 18.50m, "USD", "https://images.example.local/catalog/aspire-mug.png", true),
        new(Guid.Parse("57db4428-e3c0-4a3c-9e9e-3ca1d5df1203"), "NOTEBOOK-A5-DOT", "Clean Architecture Notebook / A5", 14.00m, "USD", "https://images.example.local/catalog/clean-architecture-notebook.png", true)
    ];

    private static readonly OrderingSeedOrder[] OrderSeeds =
    [
        new(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OrderStatus.Submitted,
            new OrderingSeedAddress("123 Demo Street", "Apt 5B", "Ward 1", "District 1", "Ho Chi Minh City", "Ho Chi Minh"),
            new OrderingSeedAddress("123 Demo Street", "Apt 5B", "Ward 1", "District 1", "Ho Chi Minh City", "Ho Chi Minh"),
            [new OrderingSeedOrderLine(Guid.Parse("1f4b328d-15bf-4dd2-9a06-7b44f78f2d01"), "DDD-HOODIE-BLK-M", 1, 59.90m, "USD")]),
        new(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            OrderStatus.Paid,
            new OrderingSeedAddress("8 Architecture Ave", null, "Ward 7", "District 3", "Ho Chi Minh City", "Ho Chi Minh"),
            null,
            [
                new OrderingSeedOrderLine(Guid.Parse("e22c58c3-4e8b-4623-90a6-a5ecb1308902"), "ASPIRE-MUG-WHT-12OZ", 2, 18.50m, "USD"),
                new OrderingSeedOrderLine(Guid.Parse("57db4428-e3c0-4a3c-9e9e-3ca1d5df1203"), "NOTEBOOK-A5-DOT", 1, 14.00m, "USD")
            ])
    ];

    private sealed record OrderingProductCacheSeed(
        Guid Id,
        string Sku,
        string Name,
        decimal CurrentPrice,
        string Currency,
        string ImageUrl,
        bool IsActive);

    private sealed record OrderingSeedOrder(
        Guid CustomerId,
        OrderStatus Status,
        OrderingSeedAddress ShippingAddress,
        OrderingSeedAddress? BillingAddress,
        OrderingSeedOrderLine[] Lines);

    private sealed record OrderingSeedOrderLine(Guid ProductId, string Sku, int Quantity, decimal UnitPrice, string Currency);

    private sealed record OrderingSeedAddress(string Line1, string? Line2, string Ward, string District, string City, string Province);
}
