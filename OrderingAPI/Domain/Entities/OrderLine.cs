using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Domain.Entities;

public sealed class OrderLine : Entity
{
    public Sku Sku { get; set; }
    public Quantity Quantity { get; set; }
    public Money Total { get; set; }
    public Guid OrderId { get; set; }
    
    public Guid ProductId { get; set; }
    public Order? Order { get; set; }

    public OrderLine()
    {
    }

    public OrderLine(Sku sku, Quantity qty, Money total)
    {
        Sku = sku;
        Quantity = qty;
        Total = total;
    }

    public void ChangeQuantity(Quantity newQty)
    {
        if (newQty.Value <= 0)
        {
            throw new DomainException("Qty must be > 0");
        }

        Quantity = newQty;
    }
}