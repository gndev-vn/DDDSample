using CatalogAPI.Features;
using CatalogAPI.Features.Products.GetProductVariantById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.UpdateProductVariant;

public static class UpdateProductVariantEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}",
                async Task<Results<Ok<ApiResponse<ProductVariantResponse>>, BadRequest<ApiResponse<object>>>>(
                    Guid id,
                    [FromBody] ProductVariantUpdateRequest request,
                    [FromServices] IValidator<ProductVariantUpdateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    if (request.Id != id)
                    {
                        return TypedResults.BadRequest(ApiResponse.Error("Id in route and model id must match"));
                    }

                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new UpdateProductVariantCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Product variant updated successfully"));
                })
            .WithName("UpdateProductVariant")
            .WithSummary("Update a product variant")
            .WithDescription("Updates an existing product variant.")
            .Accepts<ProductVariantUpdateRequest>("application/json")
            .Produces<ApiResponse<ProductVariantResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


