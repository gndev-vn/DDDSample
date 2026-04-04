using IdentityAPI.Features;
using IdentityAPI.Features.Auth.GoogleLogin;
using IdentityAPI.Features.Auth.Login;
using IdentityAPI.Features.Auth.Register;
using IdentityAPI.Features.Roles.AssignRole;
using IdentityAPI.Features.Roles.CreateRole;
using IdentityAPI.Features.Roles.GetRoles;
using IdentityAPI.Features.Roles.UpdateRolePermissions;
using IdentityAPI.Features.Users.CreateUser;
using IdentityAPI.Features.Users.DeleteUser;
using IdentityAPI.Features.Users.GetUser;
using IdentityAPI.Features.Users.GetUsers;
using IdentityAPI.Features.Users.UpdateUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authentication;
using Shared.Models;

namespace DDDSample.Tests.Identity;

public sealed class IdentityEndpointContractTests
{
    [Theory]
    [InlineData("Register", typeof(RegisterRequest))]
    [InlineData("Login", typeof(LoginRequest))]
    [InlineData("GoogleLogin", typeof(GoogleLoginRequest))]
    [InlineData("CreateRole", typeof(CreateRoleRequest))]
    [InlineData("AssignRole", typeof(AssignRolesRequest))]
    [InlineData("UpdateRolePermissions", typeof(UpdateRolePermissionsRequest))]
    [InlineData("CreateUser", typeof(CreateUserRequest))]
    [InlineData("UpdateUser", typeof(UpdateUserRequest))]
    public void Write_Endpoints_UseExpectedRequestContracts(string endpointName, Type requestType)
    {
        var endpoint = GetEndpointByName(endpointName);
        var acceptsMetadata = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();

        Assert.NotNull(acceptsMetadata);
        Assert.Equal(requestType, acceptsMetadata!.RequestType);
    }

    [Theory]
    [InlineData("Logout")]
    [InlineData("CreateRole")]
    [InlineData("AssignRole")]
    [InlineData("UpdateRolePermissions")]
    [InlineData("GetRoles")]
    [InlineData("GetUsers")]
    [InlineData("CreateUser")]
    [InlineData("UpdateUser")]
    [InlineData("DeleteUser")]
    [InlineData("GetUser")]
    [InlineData("GetCurrentUser")]
    public void Protected_Endpoints_RequireAuthorization(string endpointName)
    {
        var endpoint = GetEndpointByName(endpointName);
        var authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToList();

        Assert.NotEmpty(authorizeData);
    }

    [Fact]
    public void GetCurrentUser_DocumentsUnauthorizedResponse()
    {
        var endpoint = GetEndpointByName("GetCurrentUser");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status401Unauthorized &&
            item.Type == typeof(ApiResponse<object>));
    }

    [Fact]
    public void GetUser_DocumentsSuccessfulResponseContract()
    {
        var endpoint = GetEndpointByName("GetUser");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status200OK &&
            item.Type == typeof(ApiResponse<GetUserResponse>));
    }

    [Fact]
    public void GetUsers_DocumentsSuccessfulResponseContract()
    {
        var endpoint = GetEndpointByName("GetUsers");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status200OK &&
            item.Type == typeof(ApiResponse<IReadOnlyList<GetUserResponse>>));
    }

    [Fact]
    public void GetRoles_DocumentsSuccessfulResponseContract()
    {
        var endpoint = GetEndpointByName("GetRoles");
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status200OK &&
            item.Type == typeof(ApiResponse<IReadOnlyList<GetRolesResponse>>));
    }

    [Theory]
    [InlineData("CreateRole")]
    [InlineData("AssignRole")]
    [InlineData("UpdateRolePermissions")]
    [InlineData("GetRoles")]
    [InlineData("GetUsers")]
    [InlineData("CreateUser")]
    [InlineData("UpdateUser")]
    [InlineData("DeleteUser")]
    public void Admin_Endpoints_DocumentForbiddenResponse(string endpointName)
    {
        var endpoint = GetEndpointByName(endpointName);
        var metadata = endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().ToList();

        Assert.Contains(metadata, item =>
            item.StatusCode == StatusCodes.Status403Forbidden &&
            item.Type == typeof(ApiResponse<object>));
    }

    [Theory]
    [InlineData("GetRoles", Permissions.Roles.View)]
    [InlineData("CreateRole", Permissions.Roles.Create)]
    [InlineData("AssignRole", Permissions.Roles.Update)]
    [InlineData("UpdateRolePermissions", Permissions.Roles.Update)]
    [InlineData("GetUsers", Permissions.Users.View)]
    [InlineData("GetUser", Permissions.Users.View)]
    [InlineData("CreateUser", Permissions.Users.Create)]
    [InlineData("UpdateUser", Permissions.Users.Update)]
    [InlineData("DeleteUser", Permissions.Users.Delete)]
    public void Identity_Admin_Endpoints_UsePermissionPolicies(string endpointName, string policyName)
    {
        var endpoint = GetEndpointByName(endpointName);
        var authorizeData = endpoint.Metadata.OfType<IAuthorizeData>().ToList();

        Assert.Contains(authorizeData, item => item.Policy == policyName);
    }

    private static RouteEndpoint GetEndpointByName(string endpointName)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.Services.AddApplicationAuthorization();

        var app = builder.Build();
        app.MapIdentityEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName == endpointName);
    }
}

