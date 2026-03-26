using FluentValidation;

namespace OrderingAPI.Features.Orders.Models;

public sealed class OrderLineModelValidator : AbstractValidator<OrderLineModel>
{
    public OrderLineModelValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);
    }
}
