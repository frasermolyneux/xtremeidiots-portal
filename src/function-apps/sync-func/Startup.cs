using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.FuncHelpers.Providers;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc;
using XtremeIdiots.Portal.SyncFunc.Redirect;

[assembly: FunctionsStartup(typeof(Startup))]

namespace XtremeIdiots.Portal.SyncFunc;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var config = builder.GetContext().Configuration;

        builder.Services.AddSingleton<IRepositoryTokenProvider, RepositoryTokenProvider>();

        builder.Services.AddRepositoryApiClient(options =>
        {
            options.ApimBaseUrl = config["apim-base-url"];
            options.ApimSubscriptionKey = config["apim-subscription-key"];
        });

        builder.Services.AddMapRedirectRepository(options =>
        {
            options.MapRedirectBaseUrl = config["map-redirect-base-url"];
            options.ApiKey = config["map-redirect-api-key"];
        });

        builder.Services.AddLogging();
    }
}