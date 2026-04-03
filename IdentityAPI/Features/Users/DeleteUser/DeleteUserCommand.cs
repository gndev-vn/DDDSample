using IdentityAPI.Domain.Identity;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Shared.Exceptions;

namespace IdentityAPI.Features.Users.DeleteUser;

public sealed record DeleteUserCommand(string UserId) : IRequest<string>;

public sealed class DeleteUserHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<DeleteUserCommand, string>
{
    public async ValueTask<string> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var deleteResult = await userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            throw new BusinessException("User deletion failed", deleteResult.Errors.Select(error => error.Description));
        }

        return request.UserId;
    }
}
