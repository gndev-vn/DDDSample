using FluentValidation;
using IdentityAPI.Domain.Identity;

namespace IdentityAPI.Features.Users.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role is required");

        RuleFor(x => x.CustomerId)
            .Must(value => string.IsNullOrWhiteSpace(value) || Guid.TryParse(value, out _))
            .WithMessage("Customer id must be a valid GUID when provided.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer selection is required when assigning the Customer role.")
            .When(x => x.Roles.Any(role => string.Equals(role?.Trim(), IdentityRoleNames.Customer, StringComparison.OrdinalIgnoreCase)));
    }
}
