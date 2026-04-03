using System.Net;
using System.Net.Http.Json;
using OrderingAPI.Features.ProductsCache.GetProductsInOrder;
using Shared.Models;

namespace OrderingAPI.Services;

public interface ICatalogProductApiClient
{
    Task<IReadOnlyList<ProductCacheModel>> GetProductsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
}

public sealed class CatalogProductApiClient(HttpClient httpClient) : ICatalogProductApiClient
{
    public async Task<IReadOnlyList<ProductCacheModel>> GetProductsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        var products = await Task.WhenAll(ids.Select(id => GetProductAsync(id, cancellationToken)));

        return products
            .Where(product => product is not null)
            .Select(product => product!)
            .ToArray();
    }

    private async Task<ProductCacheModel?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/Products/{productId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CatalogProductResponse>>(cancellationToken: cancellationToken);
        var product = payload?.Data;
        if (product is null)
        {
            return null;
        }

        return new ProductCacheModel
        {
            Id = product.Id,
            Sku = product.Slug,
            Name = product.Name,
            Currency = product.Currency,
            CurrentPrice = product.BasePrice,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive
        };
    }

    private sealed class CatalogProductResponse
    {
        public Guid Id { get; init; }
        public decimal BasePrice { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public string ImageUrl { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
