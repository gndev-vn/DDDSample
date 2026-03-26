using CatalogAPI.Features.Products.Commands;
using CatalogAPI.Features.Products.Models;
using CatalogAPI.Features.Products.Queries;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductVariantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await mediator.Send(new GetProductVariantsQuery());
        return Ok(ApiResponse.Success(products, "Products retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await mediator.Send(new GetProductVariantByIdQuery(id));
        if (product == null)
        {
            return NotFound(ApiResponse.Error("Product not found"));
        }

        return Ok(ApiResponse.Success(product, "Product retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductVariantCreateRequest model)
    {
        var result = await mediator.Send(new CreateProductVariantCommand(model));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, "Product variant created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductVariantUpdateRequest model)
    {
        if (model.Id != id)
        {
            return BadRequest("Id in route and model id not match");
        }
        var result = await mediator.Send(new UpdateProductVariantCommand(model));
        return Ok(ApiResponse.Success(result, "Product updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteProductCommand(id));
        return Ok(ApiResponse.Success("Product deleted successfully"));
    }
}