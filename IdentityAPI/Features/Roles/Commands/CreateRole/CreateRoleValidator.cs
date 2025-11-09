using FluentValidation;
using IdentityAPI.Features.Roles.Models;

namespace IdentityAPI.Features.Roles.Commands.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }
}