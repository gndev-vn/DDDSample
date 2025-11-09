using Shared.Models;

namespace OrderingAPI.Features.Orders.Models;

public class OrderLineModel : ModelBase
{
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public MoneyModel UnitPrice { get; set; }
}