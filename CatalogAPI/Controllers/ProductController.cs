using CatalogAPI.Features.Product.Commands;
using CatalogAPI.Features.Product.Models;
using CatalogAPI.Features.Product.Queries;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await mediator.Send(new GetProductsQuery());
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id));
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    [Authorize] // Require authentication
    public async Task<IActionResult> Create([FromBody] ProductModel product)
    {
        var result = await mediator.Send(new CreateProductCommand(product));
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize] // Require authentication
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductModel model)
    {
        model.Id = id; // Ensure ID from route is used
        var result = await mediator.Send(new UpdateProductCommand(model));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")] // Require Admin role
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            UserId = currentUser.UserId,
            Username = currentUser.Username,
            Email = currentUser.Email,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Roles = currentUser.Roles,
            IsAuthenticated = currentUser.IsAuthenticated
        });
    }
}