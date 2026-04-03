using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.ProductsCache.GetProductsInOrder;

namespace OrderingAPI.Services;

public interface IProductLookupService
{
    Task<IReadOnlyList<ProductCacheModel>> GetManyAsync(IEnumerable<Guid> items, CancellationToken ct = default);
}

public sealed class ProductLookupService(HybridCache cache, AppDbContext db, ICatalogProductApiClient catalog) : IProductLookupService
{
    private static string Key(Guid id) => $"prd:{id:N}";

    public async Task<IReadOnlyList<ProductCacheModel>> GetManyAsync(IEnumerable<Guid> items, CancellationToken ct = default)
    {
        var keys = items.Distinct().ToArray();
        if (keys.Length == 0)
        {
            return [];
        }

        var result = new List<ProductCacheModel>(keys.Length);
        var missingProductsInCache = new List<Guid>();
        foreach (var key in keys)
        {
            var cachedProduct = await cache.GetOrCreateAsync<ProductCacheModel?>(Key(key), async token => await GetFromDbAsync(key, token), cancellationToken: ct);
            if (cachedProduct == null)
            {
                missingProductsInCache.Add(key);
            }
            else
            {
                result.Add(cachedProduct);
            }
        }

        var liveProducts = await GetLiveManyAsync(missingProductsInCache);
        if (liveProducts.Count > 0)
        {
            result.AddRange(liveProducts);
            await UpsertManyAsync(db, liveProducts, ct);

            foreach (var product in liveProducts)
            {
                await cache.SetAsync(Key(product.Id), product, cancellationToken: ct);
            }
        }

        return result;
    }

    private async Task<ProductCacheModel?> GetFromDbAsync(Guid productId, CancellationToken ct)
    {
        return await db.ProductCaches
            .Where(p => p.Id == productId)
            .Select(x => new ProductCacheModel
            {
                Id = x.Id,
                Sku = x.Sku,
                Name = x.Name,
                Currency = x.Currency,
                CurrentPrice = x.CurrentPrice,
                ImageUrl = x.ImageUrl,
                IsActive = x.IsActive,
                LastUpdatedUtc = x.LastUpdatedUtc
            }).FirstOrDefaultAsync(ct);
    }

    private async Task<List<ProductCacheModel>> GetLiveManyAsync(List<Guid> needLive)
    {
        var batch = await catalog.GetProductsAsync(needLive);
        return batch.Select(x => new ProductCacheModel
        {
            Id = x.Id,
            Sku = x.Sku,
            Name = x.Name,
            Currency = x.Currency,
            CurrentPrice = x.CurrentPrice,
            ImageUrl = x.ImageUrl,
            IsActive = x.IsActive
        }).ToList();
    }

    private static async Task UpsertManyAsync(AppDbContext db, IEnumerable<ProductCacheModel> items, CancellationToken ct)
    {
        var incoming = items.ToList();
        if (incoming.Count == 0)
        {
            return;
        }

        var ids = incoming.Select(x => x.Id).Distinct().ToArray();
        var existingProducts = await db.ProductCaches.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        var now = DateTime.UtcNow;
        foreach (var pr in incoming)
        {
            if (existingProducts.TryGetValue(pr.Id, out var existing))
            {
                existing.Sku = pr.Sku;
                existing.Name = pr.Name;
                existing.Currency = pr.Currency;
                existing.CurrentPrice = pr.CurrentPrice;
                existing.ImageUrl = pr.ImageUrl;
                existing.IsActive = pr.IsActive;
                existing.LastUpdatedUtc = now;
                existing.UpdatedAtUtc = now;
            }
            else
            {
                db.ProductCaches.Add(new ProductCache
                {
                    Id = pr.Id,
                    Sku = pr.Sku,
                    Name = pr.Name,
                    Currency = pr.Currency,
                    CurrentPrice = pr.CurrentPrice,
                    ImageUrl = pr.ImageUrl,
                    IsActive = pr.IsActive,
                    LastUpdatedUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
