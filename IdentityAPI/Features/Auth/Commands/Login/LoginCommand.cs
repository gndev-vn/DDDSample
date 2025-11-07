using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using IdentityAPI.Settings;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityAPI.Features.Auth.Commands.Login;

public record LoginCommand(
    LoginRequest Request
) : IRequest<LoginResponse>;

public class LoginHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async ValueTask<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Request.Email);
        if (user == null)
        {
            return new LoginResponse(
                Success: false,
                Message: "Invalid email or password"
            );
        }

        // Check password using UserManager instead of SignInManager
        var passwordValid = await userManager.CheckPasswordAsync(user, request.Request.Password);
        if (!passwordValid)
        {
            return new LoginResponse(
                Success: false,
                Message: "Invalid email or password"
            );
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