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
    public Money Total => _lines.Count == 0
        ? Money.Zero("VND")
        : _lines.Select(line => line.Total * line.Quantity.Value).Aggregate((sum, next) => sum + next);
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string? CustomerPhone { get; private set; }

    public Order()
    {
    }

    private Order(
        Guid customerId,
        string customerName,
        string customerEmail,
        string? customerPhone,
        Address shippingAddress,
        IEnumerable<OrderLine> lines,
        Address? billingAddress)
    {
        ArgumentNullException.ThrowIfNull(shippingAddress);

        var list = lines.ToList();
        if (list.Count == 0)
        {
            throw new DomainException("Order must have at least one line");
        }

        ApplyCustomerSnapshot(customerId, customerName, customerEmail, customerPhone);
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Status = OrderStatus.Submitted;
        AddLinesInternal(list, raiseEvents: true);

        AddDomainEvent(new OrderCreatedDomainEvent
        {
            Id = Id,
            Total = Total.Amount,
            Currency = Total.Currency,
            ShippingAddress = ShippingAddress.ToString(),
            BillingAddress = BillingAddress?.ToString() ?? string.Empty
        });
    }

    public void AddLines(IEnumerable<OrderLine> lines)
    {
        EnsureModifiable();
        AddLinesInternal(lines, raiseEvents: true);
    }

    public void AddLine(Sku sku, Quantity qty, Money unitPrice)
    {
        EnsureModifiable();
        AddLineInternal(new OrderLine(sku, qty, unitPrice), raiseEvent: true);
    }

    public void ChangeCustomer(Guid customerId, string customerName, string customerEmail, string? customerPhone)
    {
        EnsureModifiable();
        ApplyCustomerSnapshot(customerId, customerName, customerEmail, customerPhone);
    }

    internal void RestoreCustomerSnapshot(Guid customerId, string customerName, string customerEmail, string? customerPhone)
    {
        ApplyCustomerSnapshot(customerId, customerName, customerEmail, customerPhone);
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
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        _lines.Clear();
        AddLinesInternal(orderLines, raiseEvents: true);
        AddDomainEvent(new OrderUpdatedDomainEvent
        {
            Id = Id,
            Total = Total.Amount,
            Currency = Total.Currency,
            ShippingAddress = ShippingAddress.ToString(),
            BillingAddress = BillingAddress?.ToString() ?? string.Empty
        });
    }

    private void EnsureModifiable()
    {
        if (Status is OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.Completed)
        {
            throw new DomainException("Order can no longer be modified");
        }
    }

    private void ApplyCustomerSnapshot(Guid customerId, string customerName, string customerEmail, string? customerPhone)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer is required");
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new DomainException("Customer name is required");
        }

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new DomainException("Customer email is required");
        }

        CustomerId = customerId;
        CustomerName = customerName.Trim();
        CustomerEmail = customerEmail.Trim();
        CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone.Trim();
    }

    private void AddLinesInternal(IEnumerable<OrderLine> lines, bool raiseEvents)
    {
        foreach (var line in lines)
        {
            AddLineInternal(line, raiseEvents);
        }

        if (_lines.Count == 0)
        {
            throw new DomainException("Order must have at least one line");
        }
    }

    private void AddLineInternal(OrderLine line, bool raiseEvent)
    {
        ArgumentNullException.ThrowIfNull(line);

        var existing = _lines.FirstOrDefault(x => x.Sku == line.Sku);
        if (existing is null)
        {
            line.Order = this;
            line.OrderId = Id;
            _lines.Add(line);
            if (raiseEvent)
            {
                AddDomainEvent(ToOrderLineAddedDomainEvent(line, line.Quantity));
            }

            return;
        }

        existing.ChangeQuantity(existing.Quantity + line.Quantity);
        if (line.ProductId != Guid.Empty)
        {
            existing.ProductId = line.ProductId;
        }

        if (raiseEvent)
        {
            AddDomainEvent(ToOrderLineAddedDomainEvent(existing, line.Quantity));
        }
    }

    private static OrderLineAddedDomainEvent ToOrderLineAddedDomainEvent(OrderLine line, Quantity addedQuantity)
    {
        return new OrderLineAddedDomainEvent
        {
            OrderId = line.OrderId,
            OrderLineId = line.Id,
            ProductId = line.ProductId,
            Quantity = addedQuantity.Value,
            Total = line.Total.Amount * addedQuantity.Value,
            Currency = line.Total.Currency
        };
    }

    public static Order Create(
        Guid customerId,
        string customerName,
        string customerEmail,
        string? customerPhone,
        IEnumerable<OrderLine> orderLines,
        Address shippingAddress,
        Address? billingAddress = null) =>
        new(customerId, customerName, customerEmail, customerPhone, shippingAddress, orderLines, billingAddress);

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


