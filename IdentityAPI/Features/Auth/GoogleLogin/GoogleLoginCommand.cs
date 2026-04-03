using IdentityAPI.Features.Auth.Login;
using IdentityAPI.Features.Auth.Services;
using IdentityAPI.Services;
using Mediator;

namespace IdentityAPI.Features.Auth.GoogleLogin;

public record GoogleLoginCommand(GoogleLoginRequest Request) : IRequest<LoginResponse>;

public class GoogleLoginHandler(
    IIdentityAccountService identityAccountService,
    ILoginResponseFactory loginResponseFactory,
    IGoogleTokenValidator googleTokenValidator)
    : IRequestHandler<GoogleLoginCommand, LoginResponse>
{
    public async ValueTask<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken requestCancellationToken)
    {
        var googleUser = await googleTokenValidator.ValidateAsync(request.Request.IdToken, requestCancellationToken);
        var account = await identityAccountService.ResolveGoogleAccountAsync(googleUser, requestCancellationToken);
        return await loginResponseFactory.CreateAsync(account.User, account.Roles, requestCancellationToken);
    }
}
