using Grpc.Core;
using GrpcShared.Order.Services;
using Shared.Extensions;

namespace PaymentAPI.Services.Grpc;

public interface IOrderGrpcClientService
{
    Task<OrderSnapshot> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class OrderGrpcClientService(OrderSvc.OrderSvcClient client) : IOrderGrpcClientService
{
    public async Task<OrderSnapshot> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetByIdAsync(new GetOrderRequest
            {
                Id = orderId.ToString()
            }, cancellationToken: cancellationToken);

            if (response.Order == null)
            {
                throw new KeyNotFoundException($"Order '{orderId}' was not returned by the ordering service");
            }

            return new OrderSnapshot(
                Guid.Parse(response.Order.OrderId),
                Guid.Parse(response.Order.CustomerId),
                response.Order.Total?.Amount ?? 0m,
                response.Order.Total?.Currency ?? string.Empty,
                response.Order.Status);
        }
        catch (Exception ex)
        {
            throw ex.ToRpcException("payment-order-lookup");
        }
    }
}

public record OrderSnapshot(Guid OrderId, Guid CustomerId, decimal Total, string Currency, string Status);
