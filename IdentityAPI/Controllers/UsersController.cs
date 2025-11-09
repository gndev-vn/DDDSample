using System.Security.Claims;
using IdentityAPI.Features.Users.Queries.GetUser;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var result = await mediator.Send(new GetUserQuery(id));
        
        if (result == null)
        {
            return NotFound(ApiResponse.Error("User not found"));
        }

        return Ok(ApiResponse.Success(result, "User retrieved successfully"));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Error("User not authenticated"));
        }

        var result = await mediator.Send(new GetUserQuery(userId));
        
        if (result == null)
        {
            return NotFound(ApiResponse.Error("User not found"));
        }

        return Ok(ApiResponse.Success(result, "Current user retrieved successfully"));
    }
}