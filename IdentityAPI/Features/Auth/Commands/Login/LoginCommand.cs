using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace IdentityAPI.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Model) : IRequest<LoginResponse>;

public class LoginHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async ValueTask<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Model.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email address");
        }

        // Check password using UserManager
        var passwordValid = await userManager.CheckPasswordAsync(user, request.Model.Password);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid account password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is disabled");
        }

        var token = await jwtTokenService.GenerateTokenAsync(user);
        var roles = await userManager.GetRolesAsync(user);

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
}