
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using MX.GeoLocation.GeoLocationApi.Client;

using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.RepositoryFunc;
using XtremeIdiots.Portal.ServersApiClient;

[assembly: FunctionsStartup(typeof(Startup))]

namespace XtremeIdiots.Portal.RepositoryFunc;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;

        builder.Services.AddRepositoryApiClient(options =>
        {
            options.ApimBaseUrl = config["repository-api-base-url"] ?? config["apim-base-url"];
            options.ApimSubscriptionKey = config["apim-subscription-key"];
            options.ApiPathPrefix = config["repository-api-path-prefix"] ?? "repository";
        });

        builder.Services.AddServersApiClient(options =>
        {
            options.ApimBaseUrl = config["servers-api-base-url"] ?? config["apim-base-url"];
            options.ApimSubscriptionKey = config["apim-subscription-key"];
            options.ApiPathPrefix = config["servers-api-path-prefix"] ?? "servers";
        });

        builder.Services.AddGeoLocationApiClient(options =>
        {
            options.ApimBaseUrl = config["geolocation_apim_base_url"];
            options.ApimSubscriptionKey = config["geolocation_apim_subscription_key"];
        });

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
    }
}