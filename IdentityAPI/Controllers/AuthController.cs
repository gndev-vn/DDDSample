using IdentityAPI.Features.Auth.Commands.Login;
using IdentityAPI.Features.Auth.Commands.Logout;
using IdentityAPI.Features.Auth.Commands.Register;
using IdentityAPI.Features.Auth.Models;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await mediator.Send(new RegisterCommand(request));
        return Ok(ApiResponse.Success(new { userId = result.UserId }, result.Message));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(new LoginCommand(request));
        return Ok(ApiResponse.Success(new
        {
            token = result.Token,
            expiresAt = result.ExpiresAt,
            user = result.User
        }, result.Message));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var result = await mediator.Send(new LogoutCommand(token));
        return Ok(ApiResponse.Success(result.Message));
    }
}