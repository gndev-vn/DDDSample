using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace OrderingAPI.Features.ProductsCache.GetProductsInOrder;

public static class GetProductsInOrderEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/orders/{id:guid}", async Task<Ok<ApiResponse<List<ProductCacheModel>>>>(Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var products = await mediator.Send(new GetProductsInOrderQuery(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(products, "Products in order retrieved successfully"));
                })
            .WithName("GetProductsInOrder")
            .WithSummary("Get cached products in an order")
            .WithDescription("Retrieves cached product details for the products referenced by an order.")
            .Produces<ApiResponse<List<ProductCacheModel>>>(StatusCodes.Status200OK);
    }
}