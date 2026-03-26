using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using Mediator;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityAPI.Features.Auth.Commands.Logout;

public record LogoutCommand(string Token) : IRequest<LogoutResponse>;

public class LogoutCommandHandler(ITokenBlacklistService blacklistService)
    : IRequestHandler<LogoutCommand, LogoutResponse>
{
    public async ValueTask<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Parse token to get expiration
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(request.Token);

        var expiresAt = jwtToken.ValidTo;
        var timeUntilExpiry = expiresAt - DateTime.UtcNow;

        if (timeUntilExpiry > TimeSpan.Zero)
        {
            // Add token to blacklist until it expires
            await blacklistService.RevokeTokenAsync(request.Token, timeUntilExpiry);
        }

        return new LogoutResponse(true, "Logged out successfully");
    }
}

