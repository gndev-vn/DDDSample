using Microsoft.AspNetCore.Authorization;
using OrderingAPI.Features.Customers.CreateCustomer;
using OrderingAPI.Features.Customers.DeleteCustomer;
using OrderingAPI.Features.Customers.GetCustomers;
using OrderingAPI.Features.Customers.UpdateCustomer;
using OrderingAPI.Features.Orders.CancelOrder;
using OrderingAPI.Features.Orders.CreateOrder;
using OrderingAPI.Features.Orders.DeleteOrder;
using OrderingAPI.Features.Orders.GetOrderById;
using OrderingAPI.Features.Orders.GetOrders;
using OrderingAPI.Features.Orders.PayOrder;
using OrderingAPI.Features.Orders.UpdateOrder;
using OrderingAPI.Features.ProductsCache.GetProductsInOrder;
using Shared.Authentication;

namespace OrderingAPI.Features;

public static class OrderingEndpoints
{
    public static IEndpointRouteBuilder MapOrderingEndpoints(this IEndpointRouteBuilder app)
    {
        var customerReads = app.MapGroup("/api/Customers")
            .WithTags("Customers")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Customers.View });
        var customerCreates = app.MapGroup("/api/Customers")
            .WithTags("Customers")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Customers.Create });
        var customerUpdates = app.MapGroup("/api/Customers")
            .WithTags("Customers")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Customers.Update });
        var customerDeletes = app.MapGroup("/api/Customers")
            .WithTags("Customers")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Customers.Delete });

        GetCustomersEndpoint.Map(customerReads);
        CreateCustomerEndpoint.Map(customerCreates);
        UpdateCustomerEndpoint.Map(customerUpdates);
        DeleteCustomerEndpoint.Map(customerDeletes);

        var orderReads = app.MapGroup("/api/Orders")
            .WithTags("Orders")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.View });
        var orderCreates = app.MapGroup("/api/Orders")
            .WithTags("Orders")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.Create });
        var orderUpdates = app.MapGroup("/api/Orders")
            .WithTags("Orders")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.Update });
        var orderDeletes = app.MapGroup("/api/Orders")
            .WithTags("Orders")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.Delete });

        GetOrdersEndpoint.Map(orderReads);
        GetOrderByIdEndpoint.Map(orderReads);
        CreateOrderEndpoint.Map(orderCreates);
        UpdateOrderEndpoint.Map(orderUpdates);
        PayOrderEndpoint.Map(orderUpdates);
        CancelOrderEndpoint.Map(orderUpdates);
        DeleteOrderEndpoint.Map(orderDeletes);

        var productsCache = app.MapGroup("/api/ProductsCache")
            .WithTags("Products Cache")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.View });

        GetProductsInOrderEndpoint.Map(productsCache);

        return app;
    }
}
