using IdentityAPI.Services;

namespace IdentityAPI.Middlewares;

public class JwtBlacklistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            // Try to get blacklist service (may not be registered in all APIs)
            var blacklistService = context.RequestServices.GetService<ITokenBlacklistService>();
            
            if (blacklistService != null)
            {
                var isRevoked = await blacklistService.IsTokenRevokedAsync(token);
                if (isRevoked)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Token has been revoked"
                    });
                    return;
                }
            }
        }

        await next(context);
    }
}