
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
            options.BaseUrl = config["apim_base_url"] ?? config["repository_base_url"];
            options.ApiKey = config["portal_repository_apim_subscription_key"];
            options.ApiPathPrefix = config["repository_api_path_prefix"] ?? "repository";
        });

        builder.Services.AddServersApiClient(options =>
        {
            options.BaseUrl = config["apim_base_url"] ?? config["servers_base_url"];
            options.ApiKey = config["portal_servers_apim_subscription_key"];
            options.ApiPathPrefix = config["servers_api_path_prefix"] ?? "servers";
        });

        builder.Services.AddGeoLocationApiClient(options =>
        {
            options.BaseUrl = config["apim_base_url"] ?? config["geolocation_base_url"];
            options.ApiKey = config["geolocation_apim_subscription_key"];
        });

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
    }
}