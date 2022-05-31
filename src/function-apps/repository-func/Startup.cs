using FM.GeoLocation.Client.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
            options.ApimBaseUrl = config["apim-base-url"];
            options.ApimSubscriptionKey = config["apim-subscription-key"];
        });

        builder.Services.AddServersApiClient(options =>
        {
            options.ApimBaseUrl = config["apim-base-url"];
            options.ApimSubscriptionKey = config["apim-subscription-key"];
        });

        builder.Services.AddGeoLocationClient(options =>
        {
            options.BaseUrl = config["geolocation-baseurl"];
            options.ApiKey = config["geolocation-apikey"];
            options.UseMemoryCache = true;
            options.BubbleExceptions = false;
            options.CacheEntryLifeInMinutes = 60;
            options.RetryTimespans = new[]
            {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5)
                };
        });

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();
        builder.Services.AddMemoryCache();
    }
}