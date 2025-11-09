using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Domain.Entities;

public sealed class OrderLine : Entity
{
    public Sku Sku { get; }
    public Quantity Quantity { get; set; }
    public Money Price { get; }
    public Guid OrderId { get; }
    
    public Guid ProductId { get; }
    public Order Order { get; }

    private OrderLine()
    {
    }

    public OrderLine(Sku sku, Quantity qty, Money price)
    {
        Sku = sku;
        Quantity = qty;
        Price = price;
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