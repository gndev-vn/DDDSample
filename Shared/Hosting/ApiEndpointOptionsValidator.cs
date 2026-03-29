using Microsoft.Extensions.Options;

namespace Shared.Hosting;

public sealed class ApiEndpointOptionsValidator : IValidateOptions<ApiEndpointOptions>
{
    public ValidateOptionsResult Validate(string? name, ApiEndpointOptions options)
    {
        var failures = new List<string>();

        ValidatePort(options.Restful.Http, "Hosting:Restful:Http", failures);
        ValidatePort(options.Grpc.Http, "Hosting:Grpc:Http", failures);

        ValidateOptionalPort(options.Restful.Https, "Hosting:Restful:Https", failures);
        ValidateOptionalPort(options.Grpc.Https, "Hosting:Grpc:Https", failures);

        var configuredPorts = new Dictionary<string, int>
        {
            ["Hosting:Restful:Http"] = options.Restful.Http,
            ["Hosting:Grpc:Http"] = options.Grpc.Http
        };

        if (options.Restful.Https is { } restfulHttps)
        {
            configuredPorts["Hosting:Restful:Https"] = restfulHttps;
        }

        if (options.Grpc.Https is { } grpcHttps)
        {
            configuredPorts["Hosting:Grpc:Https"] = grpcHttps;
        }

        foreach (var duplicate in configuredPorts.GroupBy(x => x.Value).Where(g => g.Count() > 1))
        {
            failures.Add($"Ports must be unique across Hosting endpoints. Duplicated port {duplicate.Key}: {string.Join(", ", duplicate.Select(x => x.Key))}");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidatePort(int port, string key, ICollection<string> failures)
    {
        if (port is < 1 or > 65535)
        {
            failures.Add($"{key} must be between 1 and 65535.");
        }
    }

    private static void ValidateOptionalPort(int? port, string key, ICollection<string> failures)
    {
        if (!port.HasValue)
        {
            return;
        }

        ValidatePort(port.Value, key, failures);
    }
}
