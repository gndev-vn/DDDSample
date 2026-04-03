using FluentValidation;
using OrderingAPI.Features.Orders.GetOrderById;
using Shared.Models;

namespace OrderingAPI.Features.Orders.UpdateOrder;

public sealed class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ShippingAddress).NotNull().SetValidator(new AddressValidator()!);
        RuleForEach(x => x.Lines).SetValidator(new OrderLineValidator());
    }

    private sealed class AddressValidator : AbstractValidator<AddressModel>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Province).NotEmpty().MaximumLength(100);
            RuleFor(x => x.District).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Ward).NotEmpty().MaximumLength(100);
        }
    }

    private sealed class OrderLineValidator : AbstractValidator<OrderLineModel>
    {
        public OrderLineValidator()
        {
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }
}