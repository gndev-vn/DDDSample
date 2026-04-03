using CatalogAPI.Features;
using CatalogAPI.Features.Products.GetProductById;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.Products.UpdateProduct;

public static class UpdateProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}",
                async Task<Results<Ok<ApiResponse<ProductResponse>>, BadRequest<ApiResponse<object>>>>(
                    Guid id,
                    [FromBody] ProductUpdateRequest request,
                    [FromServices] IValidator<ProductUpdateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    if (request.Id != id)
                    {
                        return TypedResults.BadRequest(ApiResponse.Error("Id in route and model id must match"));
                    }

                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new UpdateProductCommand(request), cancellationToken);
                    return TypedResults.Ok(ApiResponse.Success(result, "Product updated successfully"));
                })
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .WithDescription("Updates an existing product.")
            .Accepts<ProductUpdateRequest>("application/json")
            .Produces<ApiResponse<ProductResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}


