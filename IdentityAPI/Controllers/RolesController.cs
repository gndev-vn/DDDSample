using IdentityAPI.Features.Roles.Commands;
using IdentityAPI.Features.Roles.Commands.AssignRole;
using IdentityAPI.Features.Roles.Commands.CreateRole;
using IdentityAPI.Features.Roles.Models;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await mediator.Send(new CreateRoleCommand(request));
        return Ok(ApiResponse.Success(new { userId = result.RoleId }, result.Message));
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignRolesRequest request)
    {
        var result = await mediator.Send(new AssignRoleCommand(request));
        return Ok(ApiResponse.Success(new { userId = result.RoleIds }, result.Message));
    }
}