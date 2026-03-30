using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Commands.Login;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Features.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

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

    [Fact]
    public async Task Handle_ValidCredentials_UsesPreloadedRolesForLoginResponse()
    {
        var expectedRoles = new[] { "User" };
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
            .ReturnsAsync(expectedRoles);

        var loginResponseFactory = new Mock<ILoginResponseFactory>();
        loginResponseFactory
            .Setup(factory => factory.CreateAsync(
                user,
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse(true, "Login successful", "signed-jwt-token"));

        var handler = new LoginHandler(userManager.Object, loginResponseFactory.Object);

        var result = await handler.Handle(new LoginCommand(new LoginRequest(user.Email!, "password")), default);

        Assert.Equal("signed-jwt-token", result.Token);
        userManager.Verify(m => m.GetRolesAsync(user), Times.Once);
        loginResponseFactory.Verify(factory => factory.CreateAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedRoles)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
