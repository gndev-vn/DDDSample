using FluentValidation;

namespace PaymentAPI.Features.Payments.Models;

public sealed class CompletePaymentRequestValidator : AbstractValidator<CompletePaymentRequest>
{
    public CompletePaymentRequestValidator()
    {
        RuleFor(x => x.TransactionReference)
            .NotEmpty()
            .MaximumLength(200);
    }
}
