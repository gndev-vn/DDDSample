using FluentValidation;

namespace OrderingAPI.Features.Orders.Models;

public sealed class CreateOrderModelValidator : AbstractValidator<CreateOrderModel>
{
    public CreateOrderModelValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .SetValidator(new AddressModelValidator()!);

        RuleFor(x => x.Lines)
            .NotEmpty();

        RuleForEach(x => x.Lines)
            .SetValidator(new OrderLineModelValidator());
    }
}
