using CatalogAPI.Domain;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TestController(IMediator mediator, AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult TestAuth()
    {
        return Ok();
    }
}