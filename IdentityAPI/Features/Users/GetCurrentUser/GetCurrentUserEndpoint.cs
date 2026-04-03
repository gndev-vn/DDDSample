using System.Security.Claims;
using IdentityAPI.Features.Users.GetUser;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Users.GetCurrentUser;

public static class GetCurrentUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/me",
                async Task<IResult>(
                    ClaimsPrincipal user,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return TypedResults.Json(ApiResponse.Error("User not authenticated"), statusCode: StatusCodes.Status401Unauthorized);
                    }

                    var result = await mediator.Send(new GetUserQuery(userId), cancellationToken);
                    if (result == null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("User not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(result, "Current user retrieved successfully"));
                })
            .WithName("GetCurrentUser")
            .WithSummary("Get the current user")
            .WithDescription("Retrieves the authenticated user profile.")
            .Produces<ApiResponse<GetUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
