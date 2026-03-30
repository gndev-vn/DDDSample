using IdentityAPI.Features.Roles.Commands;
using IdentityAPI.Features.Roles.Commands.AssignRole;
using IdentityAPI.Features.Roles.Commands.CreateRole;
using IdentityAPI.Features.Roles.Models;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace IdentityAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await mediator.Send(new CreateRoleCommand(request));
        return Ok(ApiResponse.Success(new { roleId = result.RoleId }, result.Message));
    }

    [HttpPost("assign")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign([FromBody] AssignRolesRequest request)
    {
        var result = await mediator.Send(new AssignRoleCommand(request));
        return Ok(ApiResponse.Success(new { roles = result.RoleIds }, result.Message));
    }
}
