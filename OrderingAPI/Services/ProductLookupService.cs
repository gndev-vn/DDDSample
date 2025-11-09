using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using OrderingAPI.Database;
using OrderingAPI.Domain;
using OrderingAPI.Domain.Entities;
using OrderingAPI.Features.ProductsCache.Models;
using OrderingAPI.Services.Grpc;

namespace OrderingAPI.Services;

public interface IProductLookupService
{
    Task<IReadOnlyList<ProductCacheModel>> GetManyAsync(IEnumerable<Guid> items, CancellationToken ct = default);
}

public sealed class ProductLookupService(
    HybridCache cache,
    AppDbContext db,
    IProductGrpcClientService catalog) : IProductLookupService
{
    private static string Key(Guid id) => $"prd:{id:N}";

    public async Task<IReadOnlyList<ProductCacheModel>> GetManyAsync(
        IEnumerable<Guid> items, CancellationToken ct = default)
    {
        var keys = items.Distinct().ToList();
        if (keys.Count == 0)
        {
            return [];
        }

        var result = new List<ProductCacheModel>(keys.Count);
        var missingProductsInCache = new List<Guid>();
        foreach (var key in keys)
        {
            var cachedProduct = await cache.GetOrCreateAsync<ProductCacheModel?>(key.ToString(),
                async (token) => await GetFromDbAsync(key, token), cancellationToken: ct);
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
        }

        await UpsertManyAsync(db, liveProducts, ct);

        foreach (var product in liveProducts)
        {
            await cache.SetAsync(Key(product.Id), product, cancellationToken: ct);
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
            Name = x.Name,
            Currency = x.Currency,
            CurrentPrice = x.CurrentPrice,
            ImageUrl = x.ImageUrl,
            IsActive = x.IsActive
        }).ToList();
    }

    private static async Task UpsertManyAsync(AppDbContext db, IEnumerable<ProductCacheModel> items,
        CancellationToken ct)
    {
        foreach (var pr in items)
        {
            var existing = await db.ProductCaches.FirstOrDefaultAsync(x => x.Id == pr.Id, ct);
            if (existing is null)
            {
                db.ProductCaches.Add(new ProductCache
                {
                    Id = pr.Id,
                    Name = pr.Name,
                    Currency = pr.Currency,
                    CurrentPrice = pr.CurrentPrice,
                    ImageUrl = pr.ImageUrl,
                    IsActive = pr.IsActive,
                    LastUpdatedUtc = DateTime.UtcNow
                });
            }
            else
            {
                db.Entry(existing).CurrentValues.SetValues(pr);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}