using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Features.Auth.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace IdentityAPI.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Model) : IRequest<LoginResponse>;

public class LoginHandler(
    UserManager<ApplicationUser> userManager,
    ILoginResponseFactory loginResponseFactory)
    : IRequestHandler<LoginCommand, LoginResponse>
{
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

        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return await loginResponseFactory.CreateAsync(user, roles, cancellationToken);
    }
}
