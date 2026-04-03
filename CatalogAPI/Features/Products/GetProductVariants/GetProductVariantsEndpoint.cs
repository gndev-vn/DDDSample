using CatalogAPI.Features.Products.GetProductVariantById;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.GetProductVariants;

public static class GetProductVariantsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<List<ProductVariantResponse>>>>([FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var variants = await mediator.Send(new GetProductVariantsQuery(), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(variants, "Product variants retrieved successfully"));
                })
            .WithName("GetProductVariants")
            .WithSummary("Get product variants")
            .WithDescription("Retrieves product variants.")
            .Produces<ApiResponse<List<ProductVariantResponse>>>(StatusCodes.Status200OK);
    }
}


