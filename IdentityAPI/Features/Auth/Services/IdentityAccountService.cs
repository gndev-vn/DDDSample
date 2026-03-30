using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Auth.Services;

public interface IIdentityAccountService
{
    Task<ApplicationUser> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<IdentityAccountResolution> ResolveGoogleAccountAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken);
}

public sealed record IdentityAccountResolution(ApplicationUser User, IReadOnlyCollection<string> Roles);

public sealed class IdentityAccountService(UserManager<ApplicationUser> userManager) : IIdentityAccountService
{
    private static readonly string[] DefaultUserRoles = [IdentityRoleNames.User];

    public async Task<ApplicationUser> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = ApplicationUser.CreateLocal(
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            DateTime.UtcNow);

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new BusinessException("Registration failed", createResult.Errors.Select(e => e.Description));
        }

        await AssignDefaultRoleAsync(user);
        return user;
    }

    public async Task<IdentityAccountResolution> ResolveGoogleAccountAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(googleUser.Email);
        if (user is null)
        {
            var provisionedUser = ApplicationUser.CreateGoogleUser(
                googleUser.Email,
                googleUser.GivenName ?? string.Empty,
                googleUser.FamilyName ?? string.Empty,
                googleUser.Subject,
                DateTime.UtcNow);

            var createResult = await userManager.CreateAsync(provisionedUser);
            if (!createResult.Succeeded)
            {
                throw new BusinessException("Google login provisioning failed.", createResult.Errors.Select(e => e.Description));
            }

            await AssignDefaultRoleAsync(provisionedUser);
            return new IdentityAccountResolution(provisionedUser, DefaultUserRoles);
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is disabled.");
        }

        if (string.IsNullOrWhiteSpace(user.GoogleId))
        {
            user.LinkGoogleAccount(googleUser.Subject, DateTime.UtcNow);

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new BusinessException("Failed to link Google account.", updateResult.Errors.Select(e => e.Description));
            }
        }

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return new IdentityAccountResolution(user, roles);
    }

    private async Task AssignDefaultRoleAsync(ApplicationUser user)
    {
        var roleResult = await userManager.AddToRoleAsync(user, IdentityRoleNames.User);
        if (!roleResult.Succeeded)
        {
            throw new BusinessException("Failed to assign default role.", roleResult.Errors.Select(e => e.Description));
        }
    }
}
