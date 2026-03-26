using CatalogAPI.Features.Products.Commands;
using CatalogAPI.Features.Products.Models;
using CatalogAPI.Features.Products.Queries;
using Grpc.Core;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Services;

public class ProductGrpcService(IMediator mediator, CatalogAPI.Domain.AppDbContext dbContext, ILogger<ProductGrpcService> logger)
    : ProductSvc.ProductSvcBase
{
    public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        var products = await mediator.Send(
            new GetProductsQuery(request.SearchName, request.SearchSlug, request.Page, request.PageSize),
            context.CancellationToken);

        return new GetProductsResponse
        {
            Products =
            {
                products.Adapt<List<ProductDto>>()
            }
        };
    }

    public override async Task<GetProductResponse?> GetById(GetProductRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
            {
                throw new ArgumentException("Invalid product id");
            }

            var product = await mediator.Send(new GetProductByIdQuery(id), context.CancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            return new GetProductResponse
            {
                Product = product.Adapt<ProductDto>()
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "GetById failed for {Id}", request.Id);
            throw ex.ToRpcException("catalog.get_by_id_failed");
        }
    }

    public override async Task<GetProductsByIdsResponse> GetProductsByIds(GetProductsByIdsRequest request,
        ServerCallContext context)
    {
        var ids = request.Ids
            .Select(id => Guid.TryParse(id, out var parsed) ? parsed : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new GetProductsByIdsResponse();
        }

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .ProjectToType<ProductDto>()
            .ToListAsync(context.CancellationToken);

        var response = new GetProductsByIdsResponse();
        response.Products.AddRange(products);
        return response;
    }

    public override async Task<ProductCreateResponse?> Create(ProductCreateRequest request, ServerCallContext context)
    {
        var product =
            await mediator.Send(new CreateProductCommand(request.Adapt<Features.Products.Models.ProductCreateRequest>()), context.CancellationToken);

        return new ProductCreateResponse
        {
            Id = product.Id.ToString()
        };
    }

    public override async Task<ProductUpdateResponse> Update(ProductUpdateRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new ArgumentException("Invalid product id");
        }

        var product = await mediator.Send(new GetProductByIdQuery(Guid.Parse(request.Id)), context.CancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        var cmd = new UpdateProductCommand(request.Adapt<Features.Products.Models.ProductUpdateRequest>());

        var updated = await mediator.Send(cmd, context.CancellationToken);
        return new ProductUpdateResponse { Id = updated.Id.ToString() };
    }

    public override async Task<ProductDeleteResponse> Delete(ProductDeleteRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new ArgumentException("Invalid product id");
        }

        var result = await mediator.Send(new DeleteProductCommand(Guid.Parse(request.Id)),
            context.CancellationToken);

        return new ProductDeleteResponse
        {
            Success = result
        };
    }
}
