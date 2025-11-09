using FluentValidation;
using IdentityAPI.Features.Roles.Models;

namespace IdentityAPI.Features.Roles.Commands.AssignRole;

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