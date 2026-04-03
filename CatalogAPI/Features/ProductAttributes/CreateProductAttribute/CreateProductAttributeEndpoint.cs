using CatalogAPI.Features.ProductAttributes.GetProductAttributes;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Features.ProductAttributes.CreateProductAttribute;

public static class CreateProductAttributeEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty,
                async Task<Created<ApiResponse<ProductAttributeResponse>>>(
                    [FromBody] ProductAttributeCreateRequest request,
                    [FromServices] IValidator<ProductAttributeCreateRequest> validator,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);
                    var result = await mediator.Send(new CreateProductAttributeCommand(request), cancellationToken);
                    return TypedResults.Created($"/api/ProductAttributes/{result.Id}",
                        ApiResponse.Success(result, "Product attribute created successfully"));
                })
            .WithName("CreateProductAttribute")
            .WithSummary("Create a reusable product attribute definition")
            .WithDescription("Creates a reusable product attribute definition that variants can reference.")
            .Accepts<ProductAttributeCreateRequest>("application/json")
            .Produces<ApiResponse<ProductAttributeResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status403Forbidden)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
    }
}
