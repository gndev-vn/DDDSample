using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Users.DeleteUser;

public static class DeleteUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id}",
                async Task<Ok<ApiResponse<object>>>(
                    string id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var deletedUserId = await mediator.Send(new DeleteUserCommand(id), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success<object>(new { userId = deletedUserId }, "User deleted successfully"));
                })
            .WithName("DeleteUser")
            .WithSummary("Delete a user")
            .WithDescription("Deletes a user account from the identity store.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
