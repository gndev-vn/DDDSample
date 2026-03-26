using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Exceptions;
using Shared.Models;

namespace IdentityAPI.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(GoogleLoginRequest Request) : IRequest<LoginResponse>;

public class GoogleLoginHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IGoogleTokenValidator googleTokenValidator,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<GoogleLoginCommand, LoginResponse>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async ValueTask<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Validate the Google ID token against Google's servers.
        // Throws UnauthorizedAccessException for invalid/expired tokens.
        var googleUser = await googleTokenValidator.ValidateAsync(request.Request.IdToken, cancellationToken);

        // Step 2: Find an existing user by verified email.
        var user = await userManager.FindByEmailAsync(googleUser.Email);

        if (user is null)
        {
            // First-time Google login — provision a local account.
            // Assumption: email ownership is proven by the Google token, so provisioning is safe.
            user = await ProvisionUserAsync(googleUser);
        }
        else
        {
            if (!user.IsActive)
                throw new UnauthorizedAccessException("User account is disabled.");

            // Link the Google ID to an existing account if not already linked.
            // Assumption: safest policy — we link on first successful Google login matching
            // the same verified email, but we never overwrite an existing GoogleId with a
            // different value (that would require explicit re-linking UX).
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = googleUser.Subject;
                user.UpdatedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }
        }

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var token = await jwtTokenService.GenerateTokenAsync(user, roles);

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
                roles
            )
        );
    }

    /// <summary>
    /// Creates a new local <see cref="ApplicationUser"/> from a verified Google identity.
    /// The account has no password — it can only be accessed via Google login.
    /// The email address is used as the username to guarantee uniqueness.
    /// </summary>
    private async Task<ApplicationUser> ProvisionUserAsync(GoogleUserInfo googleUser)
    {
        var user = new ApplicationUser
        {
            // Use email as username: it is unique by Identity config and avoids collision logic.
            UserName = googleUser.Email,
            Email = googleUser.Email,
            FirstName = googleUser.GivenName ?? string.Empty,
            LastName = googleUser.FamilyName ?? string.Empty,
            GoogleId = googleUser.Subject,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // CreateAsync without a password — this account is Google-only.
        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new BusinessException("Google login provisioning failed.", errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description);
            throw new BusinessException("Failed to assign role during Google login.", errors);
        }

        return user;
    }
}
