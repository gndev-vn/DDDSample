using Mediator;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Shared.Services;

namespace IdentityAPI.Features.Auth.Commands.Logout;

public class LogoutHandler(ITokenBlacklistService blacklistService)
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