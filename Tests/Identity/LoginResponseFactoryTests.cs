using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Services;
using IdentityAPI.Services;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Models;

namespace DDDSample.Tests.Identity;

public sealed class LoginResponseFactoryTests
{
    [Fact]
    public async Task CreateAsync_UsesProvidedRolesForTokenAndResponse()
    {
        var expectedRoles = new[] { "User", "Admin" };
        var user = ApplicationUser.CreateLocal("alice", "alice@example.com", "Alice", "Smith", DateTime.UtcNow);
        user.Id = Guid.NewGuid();
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "unit-test-secret-key-32chars-long!!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        });

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService
            .Setup(service => service.GenerateTokenAsync(
                user,
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles))))
            .ReturnsAsync("signed-jwt-token");

        var factory = new LoginResponseFactory(jwtTokenService.Object, jwtSettings);

        var response = await factory.CreateAsync(user, expectedRoles, default);

        Assert.Equal("signed-jwt-token", response.Token);
        Assert.Equal(expectedRoles, response.User!.Roles);
        jwtTokenService.Verify(service => service.GenerateTokenAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles))), Times.Once);
    }
}
