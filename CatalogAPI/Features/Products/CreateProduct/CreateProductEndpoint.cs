using CatalogAPI.Features;
using CatalogAPI.Features.Products.GetProductById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.CreateProduct;

public static class CreateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<ProductResponse>>>(
                    [FromBody] ProductCreateRequest request,
                    [FromServices] IValidator<ProductCreateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateProductCommand(request), cancellationToken);
                    return TypedResults.Created($"/api/Products/{result.Id}",
                        ApiResponse.Success(result, "Product created successfully"));
                })
            .WithName("CreateProduct")
            .WithSummary("Create a product")
            .WithDescription("Creates a product and returns the created resource.")
            .Accepts<ProductCreateRequest>("application/json")
            .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


