using IdentityAPI.Features.Auth.Models;
using Mediator;
using Shared.Services;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityAPI.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string Token) : IRequest<LogoutResponse>;

public sealed class LogoutCommandHandler(ITokenBlacklistService blacklistService)
    : IRequestHandler<LogoutCommand, LogoutResponse>
{
    public async ValueTask<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(request.Token);

        var expiresAt = jwtToken.ValidTo;
        var timeUntilExpiry = expiresAt - DateTime.UtcNow;

        if (timeUntilExpiry > TimeSpan.Zero)
        {
            await blacklistService.RevokeTokenAsync(request.Token, timeUntilExpiry);
        }

        return new LogoutResponse(true, "Logged out successfully");
    }
}
