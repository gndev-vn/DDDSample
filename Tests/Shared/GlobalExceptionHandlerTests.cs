using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Exceptions;
using Shared.Middleware;

namespace DDDSample.Tests.Middleware;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task InvokeAsync_WhenDomainExceptionThrown_ReturnsSpecificMessage()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = _ => throw new DomainException("Order can no longer be modified");
        var handler = new GlobalExceptionHandler(next, NullLogger<GlobalExceptionHandler>.Instance);

        await handler.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var payload = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Contains("Order can no longer be modified", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("An error occurred while processing your request", payload, StringComparison.Ordinal);
    }
}

