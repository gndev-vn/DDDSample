using FluentValidation;
using IdentityAPI.Features.Users.GetUser;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Features.Users.CreateUser;

public static class CreateUserEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<GetUserResponse>>>(
                    [FromBody] CreateUserRequest request,
                    [FromServices] IValidator<CreateUserRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateUserCommand(request), cancellationToken);
                    return TypedResults.Created($"/api/Users/{result.Id}", ApiResponse.Success(result, "User created successfully"));
                })
            .WithName("CreateUser")
            .WithSummary("Create a user")
            .WithDescription("Creates a new local identity user for administration.")
            .Accepts<CreateUserRequest>("application/json")
            .Produces<ApiResponse<GetUserResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
