using OrderingAPI.Domain.Events;
using Shared.Enums;
using Shared.Exceptions;
using Shared.Models;
using Shared.ValueObjects;

namespace OrderingAPI.Domain.Entities;

public class Order : EntityWithEvents
{
    private readonly List<OrderLine> _lines = [];
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public Address? BillingAddress { get; private set; }
    public Money Total => _lines.Aggregate(Money.Zero("VND"), (sum, l) => sum + l.Total);
    public Guid CustomerId { get; set; }

    public Order()
    {
    }

    private Order(Address address, IEnumerable<OrderLine> lines)
    {
        if (address == null)
        {
            throw new DomainException("Address required");
        }

        var list = lines.ToList();
        if (list.Count == 0)
        {
            throw new DomainException("Order must have at least one line");
        }

        ShippingAddress = address;
        _lines.AddRange(list);
        Status = OrderStatus.Submitted;

        AddDomainEvent(new OrderCreatedDomainEvent
        {
            Id = Id,
            Total = Total.Amount,
            Currency = Total.Currency,
            ShippingAddress = ShippingAddress.ToString(),
            BillingAddress = BillingAddress != null ? BillingAddress.ToString() :  string.Empty
        });
    }

    public void AddLines(IEnumerable<OrderLine> lines)
    {
        EnsureModifiable();
        foreach (var line in lines)
        {
            AddLine(line.Sku, line.Quantity, line.Total);
        }
    }

    public void AddLine(Sku sku, Quantity qty, Money unitPrice)
    {
        EnsureModifiable();
        var existing = _lines.FirstOrDefault(x => x.Sku == sku);
        if (existing is null)
        {
            _lines.Add(new OrderLine(sku, qty, unitPrice));
        }
        else
        {
            existing.ChangeQuantity(existing.Quantity + qty);
        }
    }

    public void Pay()
    {
        EnsureModifiable();

        Status = OrderStatus.Paid;
        AddDomainEvent(new OrderPaidDomainEvent
        {
            Id = Id,
            Total = Total.Amount,
            Currency = Total.Currency
        });
    }

    public void Update(Address shippingAddress, IEnumerable<OrderLine> orderLines)
    {
        EnsureModifiable();
        ShippingAddress = shippingAddress;
        _lines.Clear();
        AddLines(orderLines);
        AddDomainEvent(new OrderUpdatedDomainEvent
        {
            Id = Id,
            Total = Total.Amount,
            Currency = Total.Currency,
            ShippingAddress = ShippingAddress.ToString(),
            BillingAddress = BillingAddress.ToString()
        });
    }

    private void EnsureModifiable()
    {
        if (Status is OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.Completed)
        {
            throw new DomainException("Order can no longer be modified");
        }
    }

    public static Order Create(Guid customerId, IEnumerable<OrderLine> orderLines, Address shippingAddress,
        Address? billingAddress = null) =>
        new Order(shippingAddress, orderLines);

    public void Cancel()
    {
        EnsureModifiable();

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCanceledDomainEvent
        {
            Id = Id
        });
    }
}