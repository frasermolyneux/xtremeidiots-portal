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
            options.ApimBaseUrl = config["apim-base-url"];
            options.ApimSubscriptionKey = config["apim-subscription-key"];
        });

        builder.Services.AddLogging();

        builder.Services.AddMemoryCache();
    }
}