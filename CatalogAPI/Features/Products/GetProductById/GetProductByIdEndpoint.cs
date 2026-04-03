using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}",
                async Task<Results<Ok<ApiResponse<ProductResponse>>, NotFound<ApiResponse<object>>>>(
                    Guid id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var product = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);
                    if (product == null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("Product not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(product, "Product retrieved successfully"));
                })
            .WithName("GetProductById")
            .WithSummary("Get a product by id")
            .WithDescription("Retrieves a product by identifier.")
            .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}


