using IdentityAPI.Features.Auth.Services;
using Mediator;

namespace IdentityAPI.Features.Auth.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<RegisterResponse>;

public class RegisterHandler(IIdentityAccountService identityAccountService)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async ValueTask<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = await identityAccountService.RegisterAsync(request.Request, cancellationToken);

        return new RegisterResponse(
            Success: true,
            Message: "User registered successfully",
            UserId: user.Id.ToString()
        );
    }
}
