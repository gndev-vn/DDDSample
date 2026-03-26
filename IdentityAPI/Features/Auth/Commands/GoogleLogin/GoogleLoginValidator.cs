using FluentValidation;
using IdentityAPI.Features.Auth.Models;

namespace IdentityAPI.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token is required.");
    }
}
