using FluentValidation;

namespace IdentityAPI.Features.Roles.AssignRole;

public class AssignRoleValidator : AbstractValidator<AssignRolesRequest>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("Roles is required");
    }
}
