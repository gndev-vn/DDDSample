using IdentityAPI.Domain.Identity;
using IdentityAPI.Services;
using IdentityAPI.Settings;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IdentityAPI.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async ValueTask<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new LoginResponse(
                Success: false,
                Message: "Invalid email or password"
            );
        }

        // Check password using UserManager instead of SignInManager
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            return new LoginResponse(
                Success: false,
                Message: "Invalid email or password"
            );
        }

        var token = await _jwtTokenService.GenerateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

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