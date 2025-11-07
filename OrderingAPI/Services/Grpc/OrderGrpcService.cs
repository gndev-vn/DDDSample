using Grpc.Core;
using GrpcShared.Order.Services;
using Mediator;
using OrderingAPI.Features.Order.Commands;
using OrderingAPI.Features.Order.Models;
using OrderingAPI.Features.Order.Queries;
using Shared.Exceptions;
using Shared.Models;

namespace OrderingAPI.Services.Grpc;

public class OrderGrpcService(IMediator mediator) : OrderSvc.OrderSvcBase
{
    public override async Task<GetOrderResponse> GetById(GetOrderRequest request, ServerCallContext ctx)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid id"));
        }

        var order = await mediator.Send(new GetOrderByIdQuery(id));
        if (order is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
        }

        return new GetOrderResponse
        {
            Order = new OrderDto
            {
                OrderId = order.Id.ToString(),
                Status = order.Status.ToString(),
                Total = new MoneyDto { Amount = order.Total.Amount, Currency = order.Total.Currency },
                CustomerId = order.CustomerId.ToString(),
            }
        };
    }

    public override async Task<GetOrdersResponse> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        var orders = await mediator.Send(new GetOrdersQuery());
        return new GetOrdersResponse
        {
            Orders =
            {
                orders.Select(order => new OrderDto
                {
                    OrderId = order.Id.ToString(),
                    Status = order.Status.ToString(),
                    Total = new MoneyDto { Amount = order.Total.Amount, Currency = order.Total.Currency },
                    CustomerId = order.CustomerId.ToString(),
                }).ToList()
            }
        };
    }

    public override async Task<CreateOrderResponse> Create(CreateOrderRequest request, ServerCallContext ctx)
    {
        try
        {
            var model = new OrderModel
            {
                ShippingAddress = new AddressModel(
                    request.ShippingAddress.Line1,
                    string.IsNullOrWhiteSpace(request.ShippingAddress.Line2)
                        ? null
                        : request.ShippingAddress.Line2,
                    request.ShippingAddress.City,
                    request.ShippingAddress.Province,
                    request.ShippingAddress.District,
                    request.ShippingAddress.Ward),
                Lines = request.Lines.Select(l => new OrderLineModel
                {
                    Sku = l.Sku,
                    Quantity = l.Quantity,
                    UnitPrice = new MoneyModel
                    {
                        Amount = l.UnitPrice.Amount,
                        Currency = l.UnitPrice.Currency
                    }
                }).ToList()
            };

            var order = await mediator.Send(new CreateOrderCommand(Guid.Parse(request.CustomerId), model.Lines,
                model.ShippingAddress, null));
            return new CreateOrderResponse
            {
                Id = order.Id.ToString(),
                Status = order.Status.ToString()
            };
        }
        catch (DomainException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<UpdateOrderResponse> Update(UpdateOrderRequest request, ServerCallContext ctx)
    {
        try
        {
            var model = new OrderModel
            {
                ShippingAddress = new AddressModel(
                    request.ShippingAddress.Line1,
                    string.IsNullOrWhiteSpace(request.ShippingAddress.Line2)
                        ? null
                        : request.ShippingAddress.Line2,
                    request.ShippingAddress.City,
                    request.ShippingAddress.Province,
                    request.ShippingAddress.District,
                    request.ShippingAddress.Ward)
            };

            var order = await mediator.Send(new UpdateOrderCommand(model));
            return new UpdateOrderResponse
            {
                Id = order.Id.ToString(),
                Status = order.Status.ToString()
            };
        }
        catch (DomainException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override async Task<DeleteOrderResponse> Delete(DeleteOrderRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid id"));
            }

            var result = await mediator.Send(new DeleteOrderCommand(id));
            return new DeleteOrderResponse
            {
                Success = result
            };
        }
        catch (DomainException e)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
        }
    }
}