using Microsoft.AspNetCore.Authorization;
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
        var orderReads = app.MapGroup("/api/Orders")
            .WithTags("Orders")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.View });
        var orderWrites = app.MapGroup("/api/Orders")
            .WithTags("Orders")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.Manage });

        GetOrdersEndpoint.Map(orderReads);
        GetOrderByIdEndpoint.Map(orderReads);
        CreateOrderEndpoint.Map(orderWrites);
        UpdateOrderEndpoint.Map(orderWrites);
        PayOrderEndpoint.Map(orderWrites);
        CancelOrderEndpoint.Map(orderWrites);
        DeleteOrderEndpoint.Map(orderWrites);

        var productsCache = app.MapGroup("/api/ProductsCache")
            .WithTags("Products Cache")
            .RequireAuthorization(new AuthorizeAttribute { Policy = Permissions.Orders.View });

        GetProductsInOrderEndpoint.Map(productsCache);

        return app;
    }
}
