using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Categories.GetCategoryById;

public static class GetCategoryByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}",
                async Task<Results<Ok<ApiResponse<Category>>, NotFound<ApiResponse<object>>>>(
                    Guid id,
                    [FromServices] AppDbContext dbContext,
                    CancellationToken cancellationToken) =>
                {
                    var category = await dbContext.Categories.FindAsync([id], cancellationToken);
                    if (category == null)
                    {
                        return TypedResults.NotFound(ApiResponse.Error("Category not found"));
                    }

                    return TypedResults.Ok(ApiResponse.Success(category, "Category retrieved successfully"));
                })
            .WithName("GetCategoryById")
            .WithSummary("Get a category by id")
            .WithDescription("Retrieves a category by identifier.")
            .Produces<ApiResponse<Category>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);
    }
}


