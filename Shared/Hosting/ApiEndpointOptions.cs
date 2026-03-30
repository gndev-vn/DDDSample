namespace Shared.Hosting;

public sealed class ApiEndpointOptions
{
    public const string SectionName = "Hosting";

    public ApiTransportEndpointOptions Restful { get; init; } = new();
    public ApiTransportEndpointOptions Grpc { get; init; } = new();
}

public sealed class ApiTransportEndpointOptions
{
    public int Http { get; init; }
    public int? Https { get; init; }
}
