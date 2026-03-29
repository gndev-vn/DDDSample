using Microsoft.Extensions.Options;
using Shared.Hosting;

namespace DDDSample.Tests.Hosting;

public sealed class ApiEndpointOptionsValidatorTests
{
    private readonly ApiEndpointOptionsValidator _validator = new();

    [Fact]
    public void Validate_WithDistinctPorts_Succeeds()
    {
        var options = new ApiEndpointOptions
        {
            Restful = new ApiTransportEndpointOptions { Http = 5000, Https = 5002 },
            Grpc = new ApiTransportEndpointOptions { Http = 5001, Https = 5003 }
        };

        var result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WithDuplicatePorts_Fails()
    {
        var options = new ApiEndpointOptions
        {
            Restful = new ApiTransportEndpointOptions { Http = 5000 },
            Grpc = new ApiTransportEndpointOptions { Http = 5000 }
        };

        var result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("Duplicated port 5000", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(70000)]
    public void Validate_WithOutOfRangePort_Fails(int port)
    {
        var options = new ApiEndpointOptions
        {
            Restful = new ApiTransportEndpointOptions { Http = port },
            Grpc = new ApiTransportEndpointOptions { Http = 5001 }
        };

        var result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("Hosting:Restful:Http", StringComparison.Ordinal));
    }
}
