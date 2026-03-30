using IdentityAPI.Configuration;
using IdentityAPI.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityAPI.Services;

public sealed class IdentitySeedService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentitySeedOptions> seedOptions,
    ILogger<IdentitySeedService> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = seedOptions.Value;
        if (!options.Enabled)
        {
            logger.LogInformation("[IdentityAPI] Identity seed is disabled.");
            return;
        }

        await EnsureRoleAsync(IdentityRoleNames.Admin, "Administrator role", cancellationToken);
        await EnsureRoleAsync(IdentityRoleNames.User, "Default user role", cancellationToken);

        await EnsureUserAsync(options.Admin, IdentityRoleNames.Admin, cancellationToken);
        await EnsureUserAsync(options.User, IdentityRoleNames.User, cancellationToken);
    }

    private async Task EnsureRoleAsync(string roleName, string description, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await roleManager.RoleExistsAsync(roleName))
        {
            logger.LogInformation("[IdentityAPI] Role {RoleName} already exists.", roleName);
            return;
        }

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = roleName,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to seed role '{roleName}': {FormatErrors(result)}");
        }

        logger.LogInformation("[IdentityAPI] Seeded role {RoleName}.", roleName);
    }

    private async Task EnsureUserAsync(SeedUserOptions seedUser, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateSeedUser(seedUser, roleName);

        var user = await userManager.FindByEmailAsync(seedUser.Email);
        if (user is null)
        {
            user = ApplicationUser.CreateLocal(
                seedUser.Username,
                seedUser.Email,
                seedUser.FirstName,
                seedUser.LastName,
                DateTime.UtcNow);

            var createResult = await userManager.CreateAsync(user, seedUser.Password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user '{seedUser.Email}': {FormatErrors(createResult)}");
            }

            logger.LogInformation("[IdentityAPI] Seeded user {Email}.", seedUser.Email);
        }
        else
        {
            logger.LogInformation("[IdentityAPI] Seed user {Email} already exists.", seedUser.Email);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
        {
            logger.LogInformation("[IdentityAPI] Seed user {Email} already has role {RoleName}.", seedUser.Email, roleName);
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to assign role '{roleName}' to seeded user '{seedUser.Email}': {FormatErrors(roleResult)}");
        }

        logger.LogInformation("[IdentityAPI] Assigned role {RoleName} to seed user {Email}.", roleName, seedUser.Email);
    }

    private static void ValidateSeedUser(SeedUserOptions seedUser, string roleName)
    {
        ArgumentNullException.ThrowIfNull(seedUser);

        if (string.IsNullOrWhiteSpace(seedUser.Username))
        {
            throw new InvalidOperationException($"Identity seed username is required for role '{roleName}'.");
        }

        if (string.IsNullOrWhiteSpace(seedUser.Email))
        {
            throw new InvalidOperationException($"Identity seed email is required for role '{roleName}'.");
        }

        if (string.IsNullOrWhiteSpace(seedUser.Password))
        {
            throw new InvalidOperationException($"Identity seed password is required for role '{roleName}'.");
        }
    }

    private static string FormatErrors(IdentityResult result)
        => string.Join(", ", result.Errors.Select(error => error.Description));
}
