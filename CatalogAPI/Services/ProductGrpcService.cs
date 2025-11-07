using CatalogAPI.Features.Product.Commands;
using CatalogAPI.Features.Product.Models;
using CatalogAPI.Features.Product.Queries;
using Grpc.Core;
using Mapster;
using Mediator;
using Shared.Extensions;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace CatalogAPI.Services.Grpc;

public class ProductGrpcService(IMediator mediator, ILogger<ProductGrpcService> logger)
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

    public override async Task<CreateProductResponse?> Create(CreateProductRequest request, ServerCallContext context)
    {
        var product =
            await mediator.Send(new CreateProductCommand(request.Adapt<ProductModel>()), context.CancellationToken);

        return new CreateProductResponse
        {
            Id = product.Id.ToString()
        };
    }

    public override async Task<UpdateProductResponse> Update(UpdateProductRequest request, ServerCallContext context)
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

        var cmd = new UpdateProductCommand(request.Adapt<ProductModel>());

        var updated = await mediator.Send(cmd, context.CancellationToken);
        return new UpdateProductResponse { Id = updated.Id.ToString() };
    }

    public override async Task<DeleteProductResponse> Delete(DeleteProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out _))
        {
            throw new ArgumentException("Invalid product id");
        }

        var result = await mediator.Send(new DeleteProductCommand(Guid.Parse(request.Id)),
            context.CancellationToken);

        return new DeleteProductResponse
        {
            Success = result
        };
    }
}