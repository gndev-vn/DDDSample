using FluentValidation;

namespace PaymentAPI.Features.Payments.Models;

public sealed class FailPaymentRequestValidator : AbstractValidator<FailPaymentRequest>
{
    public FailPaymentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
