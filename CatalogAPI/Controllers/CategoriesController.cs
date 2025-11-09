using CatalogAPI.Database;
using CatalogAPI.Domain;
using CatalogAPI.Features.Categories.Commands;
using CatalogAPI.Features.Categories.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(IMediator mediator, AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await dbContext.Categories.ToListAsync();
        return Ok(ApiResponse.Success(categories, "Categories retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await dbContext.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(ApiResponse.Error("Category not found"));
        }

        return Ok(ApiResponse.Success(category, "Category retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryModel category)
    {
        var result = await mediator.Send(new CreateCategoryCommand(category));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse.Success(result, "Category created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryModel model)
    {
        model.Id = id;
        var result = await mediator.Send(new UpdateCategoryCommand(model));
        return Ok(ApiResponse.Success(result, "Category updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
        return Ok(ApiResponse.Success("Category deleted successfully"));
    }
}