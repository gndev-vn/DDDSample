using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Roles.UpdateRolePermissions;

public static class UpdateRolePermissionsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{roleId}/permissions",
                async Task<IResult>(
                    [FromRoute] string roleId,
                    [FromBody] UpdateRolePermissionsRequest request,
                    [FromServices] IValidator<UpdateRolePermissionsRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new UpdateRolePermissionsCommand(roleId, request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(new { roleId = result.RoleId }, result.Message));
                })
            .WithName("UpdateRolePermissions")
            .WithSummary("Update role permissions")
            .WithDescription("Updates the permissions assigned to a role.")
            .Accepts<UpdateRolePermissionsRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Policy = Shared.Authentication.Permissions.Roles.Update });
    }
}

