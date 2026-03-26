using IdentityAPI.Features.Auth.Commands.Logout;
using IdentityAPI.Services;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DDDSample.Tests.Identity;

/// <summary>
/// Regression tests for LogoutCommandHandler.
///
/// Background: a previous commit added alias using directives to IdentityAPI/Program.cs:
///   using ITokenBlacklistService = Shared.Services.ITokenBlacklistService;
///   using TokenBlacklistService  = Shared.Services.TokenBlacklistService;
///
/// This caused the DI registration to bind Shared.Services.ITokenBlacklistService instead
/// of IdentityAPI.Services.ITokenBlacklistService.  Because they are distinct CLR types,
/// the container could not satisfy LogoutCommandHandler's constructor and threw:
///   "Unable to resolve service for type 'IdentityAPI.Services.ITokenBlacklistService'"
///
/// Assumption: the handler must continue to depend on IdentityAPI.Services.ITokenBlacklistService
/// (not the Shared copy) until a deliberate migration consolidates the two interfaces.
/// </summary>
public sealed class LogoutCommandHandlerTests
{
    // Produces a minimal, signed JWT whose ValidTo is set to the supplied expiry.
    private static string CreateJwtToken(DateTime expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("unit-test-secret-key-32chars-long!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, "user-123")],
            expires: expiry,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── Regression: handler must accept IdentityAPI.Services.ITokenBlacklistService ──────────
    // If Program.cs accidentally maps the name to Shared.Services.ITokenBlacklistService via
    // alias usings, this test still compiles and passes because it uses the fully-qualified
    // IdentityAPI type directly.  It acts as a compile-time contract check.
    [Fact]
    public async Task Handle_DependsOnIdentityApiTokenBlacklistService_NotSharedVersion()
    {
        var blacklist = new Mock<IdentityAPI.Services.ITokenBlacklistService>();
        var handler = new LogoutCommandHandler(blacklist.Object);
        var token = CreateJwtToken(DateTime.UtcNow.AddMinutes(5));

        var result = await handler.Handle(new LogoutCommand(token), CancellationToken.None);

        Assert.True(result.Success);
    }

    // ── Happy path ────────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_ValidToken_RevokesTokenWithRemainingLifetimeAndReturnsSuccess()
    {
        var blacklist = new Mock<ITokenBlacklistService>();
        var handler = new LogoutCommandHandler(blacklist.Object);
        var token = CreateJwtToken(DateTime.UtcNow.AddMinutes(15));

        var result = await handler.Handle(new LogoutCommand(token), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEmpty(result.Message);
        blacklist.Verify(
            s => s.RevokeTokenAsync(token, It.Is<TimeSpan>(ts => ts > TimeSpan.Zero)),
            Times.Once);
    }

    // ── Edge case: token already expired ─────────────────────────────────────────────────────
    // The handler must not attempt to blacklist a token whose remaining lifetime is non-positive
    // (the cache entry would expire immediately and the write would be wasted).
    [Fact]
    public async Task Handle_AlreadyExpiredToken_SkipsRevocationAndReturnsSuccess()
    {
        var blacklist = new Mock<ITokenBlacklistService>();
        var handler = new LogoutCommandHandler(blacklist.Object);
        var token = CreateJwtToken(DateTime.UtcNow.AddMinutes(-1));

        var result = await handler.Handle(new LogoutCommand(token), CancellationToken.None);

        Assert.True(result.Success);
        blacklist.Verify(
            s => s.RevokeTokenAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    // ── Blacklist entry lifetime matches token remainder ──────────────────────────────────────
    // Consistency implication: the cache TTL must not exceed the token's remaining lifetime or
    // a revoked (but since-expired) token could still be treated as revoked after a new token
    // with the same signed payload is issued.  Conversely, a TTL shorter than the remaining
    // lifetime would allow a revoked token to pass validation after the cache entry expires.
    // This test guards the upper bound by verifying the TTL is strictly less than 16 minutes
    // when the token expires in 15 minutes.
    [Fact]
    public async Task Handle_ValidToken_RevokesWithTtlNoGreaterThanTokenRemainingLifetime()
    {
        var blacklist = new Mock<ITokenBlacklistService>();
        var handler = new LogoutCommandHandler(blacklist.Object);
        var token = CreateJwtToken(DateTime.UtcNow.AddMinutes(15));

        await handler.Handle(new LogoutCommand(token), CancellationToken.None);

        blacklist.Verify(
            s => s.RevokeTokenAsync(token, It.Is<TimeSpan>(ts => ts > TimeSpan.Zero && ts < TimeSpan.FromMinutes(16))),
            Times.Once);
    }
}
