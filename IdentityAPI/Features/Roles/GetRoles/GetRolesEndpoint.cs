using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Roles.GetRoles;

public static class GetRolesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<IReadOnlyList<GetRolesResponse>>>>(
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new GetRolesQuery(), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Roles retrieved successfully"));
                })
            .WithName("GetRoles")
            .WithSummary("Get roles")
            .WithDescription("Retrieves all identity roles.")
            .Produces<ApiResponse<IReadOnlyList<GetRolesResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden);
    }
}
