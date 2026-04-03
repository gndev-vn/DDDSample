using FluentValidation;
using IdentityAPI.Features.Users.GetUser;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Users.UpdateUser;

public static class UpdateUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id}",
                async Task<Results<Ok<ApiResponse<GetUserResponse>>, BadRequest<ApiResponse<object>>>>(
                    string id,
                    [FromBody] UpdateUserRequest request,
                    [FromServices] IValidator<UpdateUserRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    if (!string.Equals(id, request.Id, StringComparison.Ordinal))
                    {
                        return TypedResults.BadRequest(ApiResponse.Error("Id in route and model id must match"));
                    }

                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new UpdateUserCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "User updated successfully"));
                })
            .WithName("UpdateUser")
            .WithSummary("Update a user")
            .WithDescription("Updates a user's profile and activation state.")
            .Accepts<UpdateUserRequest>("application/json")
            .Produces<ApiResponse<GetUserResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
