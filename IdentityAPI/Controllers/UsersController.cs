using System.Security.Claims;
using IdentityAPI.Features.Users.Queries.GetUser;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserResponse>> GetUser(string id)
    {
        var result = await mediator.Send(new GetUserQuery(id));
        
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<GetUserResponse>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await mediator.Send(new GetUserQuery(userId));
        
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}