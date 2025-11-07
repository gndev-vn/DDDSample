using Shared.Common;
using Shared.Enums;
using Shared.Models;

namespace OrderingAPI.Features.Order.Models;

public class OrderModel : ModelBase
{
    public MoneyModel Total { get; init; }

    public OrderStatus Status { get; init; }

    public AddressModel ShippingAddress { get; set; }

    public List<OrderLineModel> Lines { get; set; }

    public Guid CustomerId { get; set; }
}
