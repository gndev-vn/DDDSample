using IdentityAPI.Domain.Identity;
using IdentityAPI.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Features.Users.GetUser;

public record GetUserQuery(string UserId) : IRequest<GetUserResponse?>;

public class GetUserHandler(
    UserManager<ApplicationUser> userManager,
    IRolePermissionService rolePermissionService) : IRequestHandler<GetUserQuery, GetUserResponse?>
{
    public async ValueTask<GetUserResponse?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await rolePermissionService.GetPermissionsAsync(roles, cancellationToken);

        return new GetUserResponse(
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
            user.UpdatedAt
        );
    }
}
