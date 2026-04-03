using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.GetProductVariantById;

public static class GetProductVariantByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}",
                async Task<Results<Ok<ApiResponse<ProductVariantResponse>>, NotFound<ApiResponse<object>>>>(
                    Guid id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var productVariant = await mediator.Send(new GetProductVariantByIdQuery(id), cancellationToken);
                    if (productVariant == null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("Product variant not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(productVariant, "Product variant retrieved successfully"));
                })
            .WithName("GetProductVariantById")
            .WithSummary("Get a product variant by id")
            .WithDescription("Retrieves a product variant by identifier.")
            .Produces<ApiResponse<ProductVariantResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}


