using CatalogAPI.Domain;
using CatalogAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace CatalogAPI.Features.Categories.GetCategories;

public static class GetCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty,
                async Task<Ok<ApiResponse<List<Category>>>>([FromServices] AppDbContext dbContext, CancellationToken cancellationToken) =>
                {
                    var categories = await dbContext.Categories.ToListAsync(cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(categories, "Categories retrieved successfully"));
                })
            .WithName("GetCategories")
            .WithSummary("Get all categories")
            .WithDescription("Retrieves all categories.")
            .Produces<ApiResponse<List<Category>>>(StatusCodes.Status200OK);
    }
}


