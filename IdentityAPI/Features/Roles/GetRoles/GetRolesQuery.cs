using IdentityAPI.Domain.Identity;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Features.Roles.GetRoles;

public sealed record GetRolesQuery() : IRequest<IReadOnlyList<GetRolesResponse>>;

public sealed class GetRolesHandler(RoleManager<ApplicationRole> roleManager) : IRequestHandler<GetRolesQuery, IReadOnlyList<GetRolesResponse>>
{
    public ValueTask<IReadOnlyList<GetRolesResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = roleManager.Roles
            .OrderBy(role => role.Name)
            .Select(role => new GetRolesResponse(
                role.Id.ToString(),
                role.Name ?? string.Empty,
                role.Description,
                role.Permissions.OrderBy(permission => permission).ToArray(),
                role.CreatedAt))
            .ToList();

        return ValueTask.FromResult<IReadOnlyList<GetRolesResponse>>(roles);
    }
}
