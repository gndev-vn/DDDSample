using CatalogAPI.Features.Products.Commands;
using CatalogAPI.Features.Products.Queries;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await mediator.Send(new GetProductsQuery());
        return Ok(ApiResponse.Success(products, "Products retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id));
        if (product == null)
        {
            return NotFound(ApiResponse.Error("Product not found"));
        }

        return Ok(ApiResponse.Success(product, "Product retrieved successfully"));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest model)
    {
        var result = await mediator.Send(new CreateProductCommand(model.Adapt<Features.Products.Models.ProductCreateRequest>()));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse.Success(result, "Product created successfully"));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] Features.Products.Models.ProductUpdateRequest model)
    {
        if (model.Id != id)
        {
            return BadRequest("Id in route and model id not match");
        }
        var result = await mediator.Send(new UpdateProductCommand(model.Adapt<Features.Products.Models.ProductUpdateRequest>()));
        return Ok(ApiResponse.Success(result, "Product updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteProductCommand(id));
        return Ok(ApiResponse.Success("Product deleted successfully"));
    }
}
