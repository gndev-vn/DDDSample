using Shared.Models;

namespace OrderingAPI.Features.Orders.Models;

public class OrderLineModel : ModelBase
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
}