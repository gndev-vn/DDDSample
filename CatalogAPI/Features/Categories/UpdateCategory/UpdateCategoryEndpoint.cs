using CatalogAPI.Features;
using CatalogAPI.Features.Categories.GetCategoryById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Categories.UpdateCategory;

public static class UpdateCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}",
                async Task<Results<Ok<ApiResponse<CategoryModel>>, BadRequest<ApiResponse<object>>>>(
                    Guid id,
                    [FromBody] CategoryUpdateRequest request,
                    [FromServices] IValidator<CategoryUpdateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    if (request.Id != id)
                    {
                        return TypedResults.BadRequest(ApiResponse.Error("Id in route and model id must match"));
                    }

                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new UpdateCategoryCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Category updated successfully"));
                })
            .WithName("UpdateCategory")
            .WithSummary("Update a category")
            .WithDescription("Updates an existing category.")
            .Accepts<CategoryUpdateRequest>("application/json")
            .Produces<ApiResponse<CategoryModel>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


