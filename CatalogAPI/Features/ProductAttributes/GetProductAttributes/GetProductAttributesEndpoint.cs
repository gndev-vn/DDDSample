using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.ProductAttributes.GetProductAttributes;

public static class GetProductAttributesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<List<ProductAttributeResponse>>>>(
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetProductAttributesQuery(), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Product attributes retrieved successfully"));
                })
            .WithName("GetProductAttributes")
            .WithSummary("List product attribute definitions")
            .WithDescription("Returns reusable product attribute definitions for variant configuration.")
            .Produces<ApiResponse<List<ProductAttributeResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
