using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Login;
using IdentityAPI.Services;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace IdentityAPI.Features.Auth.Services;

public interface ILoginResponseFactory
{
    Task<LoginResponse> CreateAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken);
}

public sealed class LoginResponseFactory(
    IJwtTokenService jwtTokenService,
    IRolePermissionService rolePermissionService,
    IOptions<JwtSettings> jwtSettings)
    : ILoginResponseFactory
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<LoginResponse> CreateAsync(ApplicationUser user, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resolvedRoles = roles as string[] ?? roles.ToArray();
        var resolvedPermissions = await rolePermissionService.GetPermissionsAsync(resolvedRoles, cancellationToken);
        var token = await jwtTokenService.GenerateTokenAsync(user, resolvedRoles, resolvedPermissions);

        return new LoginResponse(
            Success: true,
            Message: "Login successful",
            Token: token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            User: new UserInfo(
                user.Id.ToString(),
                user.UserName ?? string.Empty,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                resolvedRoles,
                resolvedPermissions,
                user.CustomerId?.ToString()
            )
        );
    }
}
