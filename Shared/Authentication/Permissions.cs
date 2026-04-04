using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Authentication;

public static class PermissionClaimTypes
{
    public const string Permission = "permission";
}

public static class ApplicationClaimTypes
{
    public const string CustomerId = "customerId";
}

public static class Permissions
{
    public static class Roles
    {
        public const string View = "identity.roles.view";
        public const string Create = "identity.roles.create";
        public const string Update = "identity.roles.update";
        public const string Delete = "identity.roles.delete";
    }

    public static class Users
    {
        public const string View = "identity.users.view";
        public const string Create = "identity.users.create";
        public const string Update = "identity.users.update";
        public const string Delete = "identity.users.delete";
    }

    public static class Categories
    {
        public const string View = "catalog.categories.view";
        public const string Create = "catalog.categories.create";
        public const string Update = "catalog.categories.update";
        public const string Delete = "catalog.categories.delete";
    }

    public static class Products
    {
        public const string View = "catalog.products.view";
        public const string Create = "catalog.products.create";
        public const string Update = "catalog.products.update";
        public const string Delete = "catalog.products.delete";
    }

    public static class Variants
    {
        public const string View = "catalog.variants.view";
        public const string Create = "catalog.variants.create";
        public const string Update = "catalog.variants.update";
        public const string Delete = "catalog.variants.delete";
    }

    public static class Customers
    {
        public const string View = "ordering.customers.view";
        public const string Create = "ordering.customers.create";
        public const string Update = "ordering.customers.update";
        public const string Delete = "ordering.customers.delete";
    }

    public static class Orders
    {
        public const string View = "ordering.orders.view";
        public const string Create = "ordering.orders.create";
        public const string Update = "ordering.orders.update";
        public const string Delete = "ordering.orders.delete";
    }

    public static class Payments
    {
        public const string View = "payment.payments.view";
        public const string Create = "payment.payments.create";
        public const string Update = "payment.payments.update";
        public const string Delete = "payment.payments.delete";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Roles.View,
        Roles.Create,
        Roles.Update,
        Roles.Delete,
        Users.View,
        Users.Create,
        Users.Update,
        Users.Delete,
        Categories.View,
        Categories.Create,
        Categories.Update,
        Categories.Delete,
        Products.View,
        Products.Create,
        Products.Update,
        Products.Delete,
        Variants.View,
        Variants.Create,
        Variants.Update,
        Variants.Delete,
        Customers.View,
        Customers.Create,
        Customers.Update,
        Customers.Delete,
        Orders.View,
        Orders.Create,
        Orders.Update,
        Orders.Delete,
        Payments.View,
        Payments.Create,
        Payments.Update,
        Payments.Delete,
    ];

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Groups =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Roles"] = [Roles.View, Roles.Create, Roles.Update, Roles.Delete],
            ["Users"] = [Users.View, Users.Create, Users.Update, Users.Delete],
            ["Categories"] = [Categories.View, Categories.Create, Categories.Update, Categories.Delete],
            ["Products"] = [Products.View, Products.Create, Products.Update, Products.Delete],
            ["Variants"] = [Variants.View, Variants.Create, Variants.Update, Variants.Delete],
            ["Customers"] = [Customers.View, Customers.Create, Customers.Update, Customers.Delete],
            ["Orders"] = [Orders.View, Orders.Create, Orders.Update, Orders.Delete],
            ["Payments"] = [Payments.View, Payments.Create, Payments.Update, Payments.Delete],
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
