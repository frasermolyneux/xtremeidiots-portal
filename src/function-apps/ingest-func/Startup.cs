using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using XtremeIdiots.Portal.IngestFunc;
using XtremeIdiots.Portal.RepositoryApiClient;

[assembly: FunctionsStartup(typeof(Startup))]

namespace XtremeIdiots.Portal.IngestFunc;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;

        builder.Services.AddRepositoryApiClient(options =>
        {
            options.BaseUrl = config["apim_base_url"] ?? config["repository_base_url"];
            options.ApiKey = config["portal_repository_apim_subscription_key"];
            options.ApiPathPrefix = config["repository_api_path_prefix"] ?? "repository";
        });

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();

        builder.Services.AddMemoryCache();
    }
}