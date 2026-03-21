using FluentValidation;
using Shared.Models;

namespace OrderingAPI.Features.Orders.Models;

public sealed class AddressModelValidator : AbstractValidator<AddressModel>
{
    public AddressModelValidator()
    {
        RuleFor(x => x.Line1)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Province)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Ward)
            .NotEmpty()
            .MaximumLength(100);
    }
}
