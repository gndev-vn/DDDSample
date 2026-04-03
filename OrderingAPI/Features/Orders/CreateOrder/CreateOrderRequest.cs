using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.CreateOrder;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public AddressModel ShippingAddress { get; set; } = null!;
    public AddressModel? BillingAddress { get; set; }
    public List<OrderLineModel> Lines { get; set; } = [];
}