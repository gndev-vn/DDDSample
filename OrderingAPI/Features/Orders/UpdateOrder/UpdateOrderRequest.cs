using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.UpdateOrder;

public class UpdateOrderRequest
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public AddressModel ShippingAddress { get; set; } = null!;
    public List<OrderLineModel> Lines { get; set; } = [];
}