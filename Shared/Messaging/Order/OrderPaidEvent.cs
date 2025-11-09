namespace Shared.Messaging.Order;

public class OrderPaidEvent
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = string.Empty;
}