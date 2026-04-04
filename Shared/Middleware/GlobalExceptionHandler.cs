using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.Models;
using InvalidOperationException = System.InvalidOperationException;

namespace Shared.Middleware;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.Error(validationEx.Message, validationEx.Errors)
            },
            NotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Response = ApiResponse.Error(notFoundEx.Message)
            },
            BusinessRuleException businessEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.Error(businessEx.Message)
            },
            BusinessException businessEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.Error(businessEx.Message, businessEx.Errors)
            },
            DomainException domainEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.Error(domainEx.Message)
            },
            ArgumentException or ArgumentNullException => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.Error("Invalid request", [exception.Message])
            },
            KeyNotFoundException => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Response = ApiResponse.Error("Resource not found", [exception.Message])
            },
            UnauthorizedAccessException => new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Response = ApiResponse.Error("Unauthorized", [exception.Message])
            },
            InvalidOperationException => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.Error("Operation failed", [exception.Message])
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse.Error("An error occurred while processing your request", [exception.Message])
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Response, options));
    }
}
