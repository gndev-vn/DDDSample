using FluentValidation;
using Shared.Authentication;

namespace IdentityAPI.Features.Roles.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleForEach(x => x.Permissions)
            .Must(permission => Permissions.All.Contains(permission))
            .WithMessage("One or more permissions are invalid.");
    }
}
