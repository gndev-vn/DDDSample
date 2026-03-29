using OrderingAPI.Features.ProductsCache.Models;

namespace OrderingAPI.Services.Grpc;

public interface IProductGrpcClientService
{
    Task<ProductCacheModel> GetProductAsync(Guid id);
    Task<IEnumerable<ProductCacheModel>> GetProductsAsync(IEnumerable<Guid> productIds);
    Task<IEnumerable<ProductCacheModel>> GetProductsAsync();
}

public class ProductGrpcClientService(ProductSvc.ProductSvcClient client) : IProductGrpcClientService
{
    public async Task<ProductCacheModel> GetProductAsync(Guid id)
    {
        var productResponse = await client.GetByIdAsync(new GetProductRequest
        {
            Id = id.ToString()
        });
        return new ProductCacheModel
        {
            Id = Guid.Parse(productResponse.Product.Id),
            Sku = productResponse.Product.Slug,
            Name = productResponse.Product.Name,
            CurrentPrice = productResponse.Product.BasePrice.Amount,
            Currency = productResponse.Product.BasePrice.Currency,
            IsActive = productResponse.Product.IsActive,
            ImageUrl = productResponse.Product.ImageUrl
        };
    }

    public async Task<IEnumerable<ProductCacheModel>> GetProductsAsync()
    {
        var productsResponse = await client.GetProductsAsync(new GetProductsRequest());
        return productsResponse.Products.Select(x => new ProductCacheModel
        {
            Id = Guid.Parse(x.Id),
            Sku = x.Slug,
            Name = x.Name,
            CurrentPrice = x.BasePrice.Amount,
            Currency = x.BasePrice.Currency,
            ImageUrl = x.ImageUrl,
            IsActive = x.IsActive
        });
    }

    public async Task<IEnumerable<ProductCacheModel>> GetProductsAsync(IEnumerable<Guid> productIds)
    {
        var request = new GetProductsByIdsRequest();
        request.Ids.AddRange(productIds.Select(id => id.ToString()));

        var productsResponse = await client.GetProductsByIdsAsync(request);
        return productsResponse.Products.Select(x => new ProductCacheModel
        {
            Id = Guid.Parse(x.Id),
            Sku = x.Slug,
            Name = x.Name,
            CurrentPrice = x.BasePrice.Amount,
            Currency = x.BasePrice.Currency,
            ImageUrl = x.ImageUrl,
            IsActive = x.IsActive
        });
    }
}
