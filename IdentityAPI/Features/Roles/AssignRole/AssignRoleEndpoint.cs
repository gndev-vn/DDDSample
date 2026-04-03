using FluentValidation;
using IdentityAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Roles.AssignRole;

public static class AssignRoleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/assign",
                async Task<IResult>(
                    [FromBody] AssignRolesRequest request,
                    [FromServices] IValidator<AssignRolesRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new AssignRoleCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(new { roles = result.RoleIds }, result.Message));
                })
            .WithName("AssignRole")
            .WithSummary("Assign roles to a user")
            .WithDescription("Replaces the roles assigned to an existing user.")
            .Accepts<AssignRolesRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
