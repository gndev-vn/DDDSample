using FluentValidation;
using Shared.Authentication;

namespace IdentityAPI.Features.Roles.UpdateRolePermissions;

public sealed class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsValidator()
    {
        RuleForEach(x => x.Permissions)
            .Must(permission => Permissions.All.Contains(permission))
            .WithMessage("One or more permissions are invalid.");
    }
}
