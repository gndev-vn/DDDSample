using FluentValidation;
using IdentityAPI.Features;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Roles.CreateRole;

public static class CreateRoleEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<IResult>(
                    [FromBody] CreateRoleRequest request,
                    [FromServices] IValidator<CreateRoleRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateRoleCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(new { roleId = result.RoleId }, result.Message));
                })
            .WithName("CreateRole")
            .WithSummary("Create a role")
            .WithDescription("Creates a new identity role.")
            .Accepts<CreateRoleRequest>("application/json")
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
