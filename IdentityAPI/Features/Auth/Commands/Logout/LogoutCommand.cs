using Mediator;

namespace IdentityAPI.Features.Auth.Commands.Logout;

public record LogoutCommand(string Token) : IRequest<LogoutResponse>;

public record LogoutResponse(bool Success, string Message);