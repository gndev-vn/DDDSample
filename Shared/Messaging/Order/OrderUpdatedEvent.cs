namespace Shared.Messaging.Order;

public class OrderUpdatedEvent
{
    public Guid Id { get; set; }

    public string ShippingAddress { get; set; }
    public string BillingAddress { get; set; }

    public decimal Total { get; set; }

    public string Currency { get; set; }
}