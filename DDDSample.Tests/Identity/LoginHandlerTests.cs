using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Commands.Login;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Models;

namespace DDDSample.Tests.Identity;

public sealed class LoginHandlerTests
{
    private static Mock<UserManager<ApplicationUser>> BuildUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }

    private static IOptions<JwtSettings> JwtOptions() =>
        Options.Create(new JwtSettings
        {
            SecretKey = "unit-test-secret-key-32chars-long!!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationInMinutes = 60
        });

    [Fact]
    public async Task Handle_ValidCredentials_UsesPreloadedRolesForJwtGeneration()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice@example.com",
            Email = "alice@example.com",
            FirstName = "Alice",
            LastName = "Smith",
            IsActive = true
        };

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(user.Email!))
            .ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "password"))
            .ReturnsAsync(true);
        userManager.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["User"]);

        var jwtService = new Mock<IJwtTokenService>();
        jwtService.Setup(s => s.GenerateTokenAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(["User"]))))
            .ReturnsAsync("signed-jwt-token");

        var handler = new LoginHandler(userManager.Object, jwtService.Object, JwtOptions());

        var result = await handler.Handle(new LoginCommand(new LoginRequest(user.Email!, "password")), default);

        Assert.Equal("signed-jwt-token", result.Token);
        userManager.Verify(m => m.GetRolesAsync(user), Times.Once);
        jwtService.Verify(s => s.GenerateTokenAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(["User"]))), Times.Once);
    }
}
