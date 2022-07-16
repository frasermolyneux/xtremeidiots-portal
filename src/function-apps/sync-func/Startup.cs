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
            options.BaseUrl = config["apim_base_url"] ?? config["repository_base_url"];
            options.ApiKey = config["portal_repository_apim_subscription_key"];
            options.ApiPathPrefix = config["repository_api_path_prefix"] ?? "repository";
        });

        builder.Services.AddMapRedirectRepository(options =>
        {
            options.MapRedirectBaseUrl = config["map_redirect_base_url"];
            options.ApiKey = config["map_redirect_api_key"];
        });

        builder.Services.AddInvisionApiClient(options =>
        {
            options.BaseUrl = config["xtremeidiots_forums_base_url"];
            options.ApiKey = config["xtremeidiots_forums_api_key"];
        });

        builder.Services.AddAdminActionTopics();

        builder.Services.AddBanFilesRepository(options =>
        {
            options.ConnectionString = config["appdata_storage_connectionstring"];
        });

        builder.Services.AddSingleton<IFtpHelper, FtpHelper>();

        builder.Services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        builder.Services.AddLogging();
    }
}