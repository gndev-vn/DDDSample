using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<RegisterResponse>;

public class RegisterHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async ValueTask<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Request.Username,
            Email = request.Request.Email,
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Request.Password);

        if (!result.Succeeded)
        {
            // Convert IdentityResult errors to ValidationException
            var errors = result.Errors.Select(e => e.Description);
            throw new BusinessException("Registration failed", errors);
        }

        // Add default role
        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description);
            throw new ValidationException("Registration failed", errors);
        }

        return new RegisterResponse(
            Success: true,
            Message: "User registered successfully",
            UserId: user.Id.ToString()
        );
    }
}