using IdentityAPI.Domain.Identity;
using IdentityAPI.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Features.Users.GetUsers;

public sealed record GetUsersQuery() : IRequest<IReadOnlyList<GetUser.GetUserResponse>>;

public sealed class GetUsersHandler(
    UserManager<ApplicationUser> userManager,
    IRolePermissionService rolePermissionService) : IRequestHandler<GetUsersQuery, IReadOnlyList<GetUser.GetUserResponse>>
{
    public async ValueTask<IReadOnlyList<GetUser.GetUserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var users = userManager.Users
            .OrderBy(user => user.Email)
            .ToList();

        var results = new List<GetUser.GetUserResponse>(users.Count);
        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var roles = await userManager.GetRolesAsync(user);
            var permissions = await rolePermissionService.GetPermissionsAsync(roles, cancellationToken);

            results.Add(new GetUser.GetUserResponse(
                user.Id.ToString(),
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.CustomerId?.ToString(),
                roles,
                permissions,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt));
        }

        return results;
    }
}
