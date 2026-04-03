using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Models;

namespace IdentityAPI.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/logout",
                async Task<Ok<ApiResponse<object>>>(
                    HttpContext httpContext,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var token = httpContext.Request.Headers.Authorization.ToString()
                        .Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);

                    var result = await mediator.Send(new LogoutCommand(token), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result.Message));
                })
            .WithName("Logout")
            .WithSummary("Log out the current user")
            .WithDescription("Revokes the current JWT by placing it on the blacklist until it expires.")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }
}
