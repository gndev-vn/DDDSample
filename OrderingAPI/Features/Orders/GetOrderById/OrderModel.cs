using Shared.Enums;
using Shared.Models;

namespace OrderingAPI.Features.Orders.GetOrderById;

public class OrderModel : ModelBase
{
    public OrderStatus Status { get; init; }
    public AddressModel? ShippingAddress { get; set; }
    public List<OrderLineModel> Lines { get; set; } = [];
    public Guid CustomerId { get; set; }
}