using IdentityAPI.Features.Auth.Commands.Login;
using IdentityAPI.Features.Auth.Commands.Register;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterCommand command)
    {
        var result = await mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return Unauthorized(result);
    }
}