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
    public async Task CreateAsync_UsesProvidedRolesAndPermissionsForTokenAndResponse()
    {
        var expectedRoles = new[] { "User", "Admin" };
        var expectedPermissions = new[] { "catalog.products.view", "ordering.orders.update" };
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
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles)),
                It.Is<IEnumerable<string>>(permissions => permissions.SequenceEqual(expectedPermissions))))
            .ReturnsAsync("signed-jwt-token");

        var rolePermissionService = new Mock<IRolePermissionService>();
        rolePermissionService
            .Setup(service => service.GetPermissionsAsync(
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPermissions);

        var factory = new LoginResponseFactory(jwtTokenService.Object, rolePermissionService.Object, jwtSettings);

        var response = await factory.CreateAsync(user, expectedRoles, default);

        Assert.Equal("signed-jwt-token", response.Token);
        Assert.Equal(expectedRoles, response.User!.Roles);
        Assert.Equal(expectedPermissions, response.User.Permissions);
        jwtTokenService.Verify(service => service.GenerateTokenAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles)),
            It.Is<IEnumerable<string>>(permissions => permissions.SequenceEqual(expectedPermissions))), Times.Once);
    }
}

