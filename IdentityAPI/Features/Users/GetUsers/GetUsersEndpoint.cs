using IdentityAPI.Features.Users.GetUser;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Users.GetUsers;

public static class GetUsersEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<IReadOnlyList<GetUserResponse>>>>(
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetUsersQuery(), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Users retrieved successfully"));
                })
            .WithName("GetUsers")
            .WithSummary("Get users")
            .WithDescription("Retrieves all users for identity administration.")
            .Produces<ApiResponse<IReadOnlyList<GetUserResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
