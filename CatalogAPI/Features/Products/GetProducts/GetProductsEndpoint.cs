using CatalogAPI.Features.Products.GetProductById;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.GetProducts;

public static class GetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<List<ProductResponse>>>>([FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var products = await mediator.Send(new GetProductsQuery(), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(products, "Products retrieved successfully"));
                })
            .WithName("GetProducts")
            .WithSummary("Get all products")
            .WithDescription("Retrieves products using the default query behavior.")
            .Produces<ApiResponse<List<ProductResponse>>>(StatusCodes.Status200OK);
    }
}


