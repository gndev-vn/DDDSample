using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace Shared.Hosting;

public static class ApiEndpointConfigurationExtensions
{
    public static WebApplicationBuilder AddCentralizedApiEndpoints(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IValidateOptions<ApiEndpointOptions>, ApiEndpointOptionsValidator>();
        builder.Services.AddOptions<ApiEndpointOptions>()
            .Bind(builder.Configuration.GetRequiredSection(ApiEndpointOptions.SectionName))
            .ValidateOnStart();

        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            var endpointOptions = context.Configuration
                .GetRequiredSection(ApiEndpointOptions.SectionName)
                .Get<ApiEndpointOptions>() ?? throw new InvalidOperationException(
                $"Missing {ApiEndpointOptions.SectionName} configuration.");

            var validationResult = new ApiEndpointOptionsValidator().Validate(Options.DefaultName, endpointOptions);
            if (validationResult.Failed)
            {
                throw new InvalidOperationException(
                    $"Invalid {ApiEndpointOptions.SectionName} configuration: {string.Join("; ", validationResult.Failures)}");
            }

            options.ListenAnyIP(endpointOptions.Restful.Http, listenOptions =>
                listenOptions.Protocols = HttpProtocols.Http1);
            options.ListenAnyIP(endpointOptions.Grpc.Http, listenOptions =>
                listenOptions.Protocols = HttpProtocols.Http2);
        });

        return builder;
    }
}
