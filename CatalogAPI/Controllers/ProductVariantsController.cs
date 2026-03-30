using CatalogAPI.Features.Products.Commands;
using CatalogAPI.Features.Products.Models;
using CatalogAPI.Features.Products.Queries;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductVariantsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves product variants.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductVariantResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var variants = await mediator.Send(new GetProductVariantsQuery());
        return Ok(ApiResponse.Success(variants, "Product variants retrieved successfully"));
    }

    /// <summary>
    /// Retrieves a product variant by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductVariantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var productVariant = await mediator.Send(new GetProductVariantByIdQuery(id));
        if (productVariant == null)
        {
            return NotFound(ApiResponse.Error("Product variant not found"));
        }

        return Ok(ApiResponse.Success(productVariant, "Product variant retrieved successfully"));
    }

    /// <summary>
    /// Creates a product variant for an existing product.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductVariantResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] ProductVariantCreateRequest model)
    {
        var result = await mediator.Send(new CreateProductVariantCommand(model));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, "Product variant created successfully"));
    }

    /// <summary>
    /// Updates a product variant.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductVariantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductVariantUpdateRequest model)
    {
        if (model.Id != id)
        {
            return BadRequest("Id in route and model id must match");
        }

        var result = await mediator.Send(new UpdateProductVariantCommand(model));
        return Ok(ApiResponse.Success(result, "Product variant updated successfully"));
    }

    /// <summary>
    /// Deletes a product variant.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteProductVariantCommand(id));
        return Ok(ApiResponse.Success("Product variant deleted successfully"));
    }
}
