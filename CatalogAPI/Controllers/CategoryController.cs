using CatalogAPI.Database;
using CatalogAPI.Domain;
using CatalogAPI.Features.Category.Commands;
using CatalogAPI.Features.Category.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(IMediator mediator, AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await dbContext.Categories.ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryModel category)
    {
        var result = await mediator.Send(new CreateCategoryCommand(category));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryModel model)
    {
        model.Id = id; // Ensure ID from route is used
        var result = await mediator.Send(new UpdateCategoryCommand(model));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}