using IdentityAPI.Features.Auth.Logout;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shared.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DDDSample.Tests.Identity;

public sealed class LogoutCommandHandlerTests
{
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

    [Fact]
    public async Task Handle_DependsOnSharedTokenBlacklistService()
    {
        var blacklist = new Mock<ITokenBlacklistService>();
        var handler = new LogoutCommandHandler(blacklist.Object);
        var token = CreateJwtToken(DateTime.UtcNow.AddMinutes(5));

        var result = await handler.Handle(new LogoutCommand(token), CancellationToken.None);

        Assert.True(result.Success);
    }

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
