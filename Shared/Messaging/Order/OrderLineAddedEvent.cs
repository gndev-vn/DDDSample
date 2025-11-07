namespace Shared.Messaging.Order;

public class OrderLineAddedEvent
{
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}