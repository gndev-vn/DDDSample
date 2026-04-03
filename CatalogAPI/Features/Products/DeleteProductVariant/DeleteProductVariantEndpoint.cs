using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.DeleteProductVariant;

public static class DeleteProductVariantEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}",
                async Task<Ok<ApiResponse<object>>>(Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new DeleteProductVariantCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success("Product variant deleted successfully"));
                })
            .WithName("DeleteProductVariant")
            .WithSummary("Delete a product variant")
            .WithDescription("Deletes a product variant by identifier.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


