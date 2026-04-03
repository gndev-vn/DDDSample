using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Users.GetUser;

public static class GetUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id}",
                async Task<IResult>(
                    string id,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetUserQuery(id), cancellationToken);
                    if (result == null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("User not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(result, "User retrieved successfully"));
                })
            .WithName("GetUser")
            .WithSummary("Get a user by id")
            .WithDescription("Retrieves a user by identifier.")
            .Produces<ApiResponse<GetUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}
