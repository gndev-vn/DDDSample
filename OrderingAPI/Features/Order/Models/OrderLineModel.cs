using Shared.Common;
using Shared.Models;

namespace OrderingAPI.Features.Order.Models;

public class OrderLineModel : ModelBase
{
    public string Sku { get; set; }
    public int Quantity { get; set; }
    public MoneyModel UnitPrice { get; set; }
}