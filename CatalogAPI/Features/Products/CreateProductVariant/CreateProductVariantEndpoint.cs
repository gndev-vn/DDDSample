using CatalogAPI.Features;
using CatalogAPI.Features.Products.GetProductVariantById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.CreateProductVariant;

public static class CreateProductVariantEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<ProductVariantResponse>>>(
                    [FromBody] ProductVariantCreateRequest request,
                    [FromServices] IValidator<ProductVariantCreateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateProductVariantCommand(request), cancellationToken);
                    return TypedResults.Created($"/api/ProductVariants/{result.Id}",
                        ApiResponse.Success(result, "Product variant created successfully"));
                })
            .WithName("CreateProductVariant")
            .WithSummary("Create a product variant")
            .WithDescription("Creates a product variant for an existing product.")
            .Accepts<ProductVariantCreateRequest>("application/json")
            .Produces<ApiResponse<ProductVariantResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


