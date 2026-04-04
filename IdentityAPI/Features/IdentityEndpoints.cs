using IdentityAPI.Features.Auth.GoogleLogin;
using IdentityAPI.Features.Auth.Login;
using IdentityAPI.Features.Auth.Logout;
using IdentityAPI.Features.Auth.Register;
using IdentityAPI.Features.Roles.AssignRole;
using IdentityAPI.Features.Roles.CreateRole;
using IdentityAPI.Features.Roles.GetRoles;
using IdentityAPI.Features.Roles.UpdateRolePermissions;
using IdentityAPI.Features.Users.CreateUser;
using IdentityAPI.Features.Users.DeleteUser;
using IdentityAPI.Features.Users.GetCurrentUser;
using IdentityAPI.Features.Users.GetUser;
using IdentityAPI.Features.Users.GetUsers;
using IdentityAPI.Features.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Shared.Authentication;

namespace IdentityAPI.Features;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/Auth")
            .WithTags("Auth");

        RegisterEndpoint.Map(auth);
        LoginEndpoint.Map(auth);
        LogoutEndpoint.Map(auth);
        GoogleLoginEndpoint.Map(auth);

        var roleReads = app.MapGroup("/api/Roles")
            .WithTags("Roles")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Roles.View });
        var roleCreates = app.MapGroup("/api/Roles")
            .WithTags("Roles")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Roles.Create });
        var roleUpdates = app.MapGroup("/api/Roles")
            .WithTags("Roles")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Roles.Update });

        GetRolesEndpoint.Map(roleReads);
        CreateRoleEndpoint.Map(roleCreates);
        AssignRoleEndpoint.Map(roleUpdates);
        UpdateRolePermissionsEndpoint.Map(roleUpdates);

        var currentUser = app.MapGroup("/api/Users")
            .WithTags("Users")
            .RequireAuthorization();
        GetCurrentUserEndpoint.Map(currentUser);

        var userReads = app.MapGroup("/api/Users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Users.View });
        var userCreates = app.MapGroup("/api/Users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Users.Create });
        var userUpdates = app.MapGroup("/api/Users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Users.Update });
        var userDeletes = app.MapGroup("/api/Users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Users.Delete });

        GetUsersEndpoint.Map(userReads);
        GetUserEndpoint.Map(userReads);
        CreateUserEndpoint.Map(userCreates);
        UpdateUserEndpoint.Map(userUpdates);
        DeleteUserEndpoint.Map(userDeletes);

        return app;
    }
}
