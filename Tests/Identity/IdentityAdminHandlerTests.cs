using IdentityAPI.Domain.Identity;
using IdentityAPI.Features.Roles.AssignRole;
using IdentityAPI.Features.Users.GetUsers;
using IdentityAPI.Features.Users.UpdateUser;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DDDSample.Tests.Identity;

public sealed class IdentityAdminHandlerTests
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

    private static Mock<RoleManager<ApplicationRole>> BuildRoleManagerMock()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            store.Object,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<ApplicationRole>>>().Object);
    }

    [Fact]
    public async Task GetUsersHandler_ReturnsUsersWithResolvedRoles()
    {
        var firstUser = ApplicationUser.CreateLocal("admin", "admin@example.com", "Admin", "User", DateTime.UtcNow);
        var secondUser = ApplicationUser.CreateLocal("manager", "manager@example.com", "Manager", "User", DateTime.UtcNow);

        var userManager = BuildUserManagerMock();
        userManager.SetupGet(manager => manager.Users).Returns(new[] { firstUser, secondUser }.AsQueryable());
        userManager.Setup(manager => manager.GetRolesAsync(firstUser)).ReturnsAsync([IdentityRoleNames.Admin]);
        userManager.Setup(manager => manager.GetRolesAsync(secondUser)).ReturnsAsync([IdentityRoleNames.User]);

        var rolePermissionService = new Mock<IRolePermissionService>();
        rolePermissionService.Setup(service => service.GetPermissionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var handler = new GetUsersHandler(userManager.Object, rolePermissionService.Object);

        var result = await handler.Handle(new GetUsersQuery(), default);

        Assert.Collection(result,
            user =>
            {
                Assert.Equal("admin@example.com", user.Email);
                Assert.Contains(IdentityRoleNames.Admin, user.Roles);
            },
            user =>
            {
                Assert.Equal("manager@example.com", user.Email);
                Assert.Contains(IdentityRoleNames.User, user.Roles);
            });
    }

    [Fact]
    public async Task UpdateUserHandler_UpdatesMutableFieldsAndActivityState()
    {
        var user = ApplicationUser.CreateLocal("jane", "jane@example.com", "Jane", "Doe", DateTime.UtcNow);
        var userManager = BuildUserManagerMock();
        userManager.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(manager => manager.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync([IdentityRoleNames.User]);

        var rolePermissionService = new Mock<IRolePermissionService>();
        rolePermissionService.Setup(service => service.GetPermissionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var handler = new UpdateUserHandler(userManager.Object, rolePermissionService.Object);

        var result = await handler.Handle(
            new UpdateUserCommand(new UpdateUserRequest(
                user.Id.ToString(),
                "jane.admin",
                "jane.admin@example.com",
                "Jane",
                "Admin",
                false)),
            default);

        Assert.Equal("jane.admin", result.Username);
        Assert.Equal("jane.admin@example.com", result.Email);
        Assert.Equal("Admin", result.LastName);
        Assert.False(result.IsActive);
        userManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task AssignRoleCommandHandler_ReplacesExistingRolesWithRequestedSet()
    {
        var user = ApplicationUser.CreateLocal("alex", "alex@example.com", "Alex", "Doe", DateTime.UtcNow);
        var userManager = BuildUserManagerMock();
        var roleManager = BuildRoleManagerMock();

        userManager.Setup(manager => manager.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync([IdentityRoleNames.User, "Legacy"]);
        userManager.Setup(manager => manager.RemoveFromRolesAsync(
                user,
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { IdentityRoleNames.User, "Legacy" }))))
            .ReturnsAsync(IdentityResult.Success);
        userManager.Setup(manager => manager.AddToRolesAsync(
                user,
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { IdentityRoleNames.Admin }))))
            .ReturnsAsync(IdentityResult.Success);

        roleManager.Setup(manager => manager.RoleExistsAsync(IdentityRoleNames.Admin)).ReturnsAsync(true);

        var handler = new AssignRoleCommandHandler(userManager.Object, roleManager.Object);

        var result = await handler.Handle(
            new AssignRoleCommand(new AssignRolesRequest(user.Id, [IdentityRoleNames.Admin])),
            default);

        Assert.Equal([IdentityRoleNames.Admin], result.RoleIds);
        userManager.Verify(manager => manager.RemoveFromRolesAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { IdentityRoleNames.User, "Legacy" }))), Times.Once);
        userManager.Verify(manager => manager.AddToRolesAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { IdentityRoleNames.Admin }))), Times.Once);
    }
}
