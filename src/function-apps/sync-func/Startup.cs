using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using XtremeIdiots.Portal.ForumsIntegration.Extensions;
using XtremeIdiots.Portal.InvisionApiClient;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.SyncFunc;
using XtremeIdiots.Portal.SyncFunc.Extensions;
using XtremeIdiots.Portal.SyncFunc.Helpers;
using XtremeIdiots.Portal.SyncFunc.Redirect;

[assembly: FunctionsStartup(typeof(Startup))]

namespace XtremeIdiots.Portal.SyncFunc;

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

        builder.Services.AddMapRedirectRepository(options =>
        {
            options.MapRedirectBaseUrl = config["map-redirect-base-url"];
            options.ApiKey = config["map-redirect-api-key"];
        });

        builder.Services.AddInvisionApiClient(options =>
        {
            options.BaseUrl = config["xtremeidiots-forums-base-url"];
            options.ApiKey = config["xtremeidiots-forums-api-key"];
        });

        builder.Services.AddAdminActionTopics();

        builder.Services.AddBanFilesRepository(options =>
        {
            options.ConnectionString = config["appdata-storage-connectionstring"];
        });

        builder.Services.AddSingleton<IFtpHelper, FtpHelper>();

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();
    }
}