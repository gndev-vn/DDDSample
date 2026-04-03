using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Authentication;

public static class PermissionClaimTypes
{
    public const string Permission = "permission";
}

public static class Permissions
{
    public static class Roles
    {
        public const string View = "identity.roles.view";
        public const string Manage = "identity.roles.manage";
    }

    public static class Users
    {
        public const string View = "identity.users.view";
        public const string Manage = "identity.users.manage";
    }

    public static class Categories
    {
        public const string View = "catalog.categories.view";
        public const string Manage = "catalog.categories.manage";
    }

    public static class Products
    {
        public const string View = "catalog.products.view";
        public const string Manage = "catalog.products.manage";
    }

    public static class Variants
    {
        public const string View = "catalog.variants.view";
        public const string Manage = "catalog.variants.manage";
    }

    public static class Orders
    {
        public const string View = "ordering.orders.view";
        public const string Manage = "ordering.orders.manage";
    }

    public static class Payments
    {
        public const string View = "payment.payments.view";
        public const string Manage = "payment.payments.manage";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Roles.View,
        Roles.Manage,
        Users.View,
        Users.Manage,
        Categories.View,
        Categories.Manage,
        Products.View,
        Products.Manage,
        Variants.View,
        Variants.Manage,
        Orders.View,
        Orders.Manage,
        Payments.View,
        Payments.Manage
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Groups =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Roles"] = [Roles.View, Roles.Manage],
            ["Users"] = [Users.View, Users.Manage],
            ["Categories"] = [Categories.View, Categories.Manage],
            ["Products"] = [Products.View, Products.Manage],
            ["Variants"] = [Variants.View, Variants.Manage],
            ["Orders"] = [Orders.View, Orders.Manage],
            ["Payments"] = [Payments.View, Payments.Manage]
        };
}

public static class AuthorizationConfigurationExtensions
{
    public static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") ||
                        context.User.HasClaim(PermissionClaimTypes.Permission, permission));
                });
            }
        });

        return services;
    }
}
