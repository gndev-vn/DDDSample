using Shared.Models;

namespace OrderingAPI.Features.Orders.Models;

public class CreateOrderModel
{
    public Guid CustomerId { get; set; }
    public AddressModel ShippingAddress { get; set; }
    public AddressModel? BillingAddress { get; set; }
    public List<OrderLineModel> Lines { get; set; } = [];
}