using Mediator;

namespace IdentityAPI.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;