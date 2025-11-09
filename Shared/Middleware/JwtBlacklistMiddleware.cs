using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace Shared.Middleware;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public JwtBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

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

        await _next(context);
    }
}