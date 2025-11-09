using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Roles.Models;
using Mapster;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(CreateRoleRequest Model) : IRequest<CreateRoleResponse>;

public class CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager) : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    public async ValueTask<CreateRoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = request.Model.Adapt<ApplicationRole>();
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new BusinessException("Failed to create role", result.Errors.Select(e => e.Description));
        }

        return new CreateRoleResponse(
            Success: true,
            Message: "Role created successfully",
            RoleId: role.Id
        );
    }
}