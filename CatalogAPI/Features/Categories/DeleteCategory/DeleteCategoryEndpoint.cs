using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Categories.DeleteCategory;

public static class DeleteCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}",
                async Task<Ok<ApiResponse<object>>>(Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
                {
                    await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success("Category deleted successfully"));
                })
            .WithName("DeleteCategory")
            .WithSummary("Delete a category")
            .WithDescription("Deletes a category by identifier.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


