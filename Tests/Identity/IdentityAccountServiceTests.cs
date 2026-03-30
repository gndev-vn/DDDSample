using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Auth.Models;
using IdentityAPI.Features.Auth.Services;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Exceptions;

namespace DDDSample.Tests.Identity;

public sealed class IdentityAccountServiceTests
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
    public async Task RegisterAsync_CreatesUserAndAssignsDefaultRole()
    {
        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "password"))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentityRoleNames.User))
            .ReturnsAsync(IdentityResult.Success);

        var service = new IdentityAccountService(userManager.Object);
        var request = new RegisterRequest("alice", "alice@example.com", "password", "Alice", "Smith");

        var user = await service.RegisterAsync(request, default);

        Assert.Equal("alice", user.UserName);
        Assert.Equal("alice@example.com", user.Email);
        userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(candidate =>
            candidate.UserName == request.Username &&
            candidate.Email == request.Email &&
            candidate.FirstName == request.FirstName &&
            candidate.LastName == request.LastName), request.Password), Times.Once);
        userManager.Verify(m => m.AddToRoleAsync(user, IdentityRoleNames.User), Times.Once);
    }

    [Fact]
    public async Task ResolveGoogleAccountAsync_NewUser_ReturnsDefaultRoleWithoutFetchingRoles()
    {
        var googleUser = new GoogleUserInfo("google-sub-123", "new@example.com", "Alice", "Smith");
        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), IdentityRoleNames.User))
            .ReturnsAsync(IdentityResult.Success);

        var service = new IdentityAccountService(userManager.Object);

        var result = await service.ResolveGoogleAccountAsync(googleUser, default);

        Assert.Equal(IdentityRoleNames.User, Assert.Single(result.Roles));
        Assert.Equal(googleUser.Subject, result.User.GoogleId);
        Assert.True(result.User.EmailConfirmed);
        userManager.Verify(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ResolveGoogleAccountAsync_ExistingUserWithoutGoogleId_LinksAccountAndFetchesRoles()
    {
        var googleUser = new GoogleUserInfo("google-sub-123", "alice@example.com", "Alice", "Smith");
        var existingUser = ApplicationUser.CreateLocal("alice@example.com", "alice@example.com", "Alice", "Smith", DateTime.UtcNow);

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync(existingUser);
        userManager.Setup(m => m.UpdateAsync(existingUser))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync(["User"]);

        var service = new IdentityAccountService(userManager.Object);

        var result = await service.ResolveGoogleAccountAsync(googleUser, default);

        Assert.Equal(googleUser.Subject, existingUser.GoogleId);
        Assert.Equal(["User"], result.Roles);
        userManager.Verify(m => m.UpdateAsync(existingUser), Times.Once);
        userManager.Verify(m => m.GetRolesAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task ResolveGoogleAccountAsync_CreateFailure_ThrowsBusinessException()
    {
        var googleUser = new GoogleUserInfo("google-sub-123", "new@example.com", "Alice", "Smith");
        var identityErrors = new[] { new IdentityError { Description = "Duplicate email" } };

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(googleUser.Email))
            .ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(identityErrors));

        var service = new IdentityAccountService(userManager.Object);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ResolveGoogleAccountAsync(googleUser, default));

        Assert.Contains("Duplicate email", exception.Errors);
    }
}
