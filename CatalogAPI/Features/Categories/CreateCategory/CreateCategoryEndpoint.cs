using CatalogAPI.Features;
using CatalogAPI.Features.Categories.GetCategoryById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Categories.CreateCategory;

public static class CreateCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<CategoryModel>>>(
                    [FromBody] CategoryCreateRequest request,
                    [FromServices] IValidator<CategoryCreateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateCategoryCommand(request), cancellationToken);
                    return TypedResults.Created($"/api/Categories/{result.Id}",
                        ApiResponse.Success(result, "Category created successfully"));
                })
            .WithName("CreateCategory")
            .WithSummary("Create a category")
            .WithDescription("Creates a category and returns the created resource.")
            .Accepts<CategoryCreateRequest>("application/json")
            .Produces<ApiResponse<CategoryModel>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


