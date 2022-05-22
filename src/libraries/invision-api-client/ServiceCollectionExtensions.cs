using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.InvisionApiClient.CoreApi;
using XtremeIdiots.Portal.InvisionApiClient.DownloadsApi;
using XtremeIdiots.Portal.InvisionApiClient.ForumsApi;

namespace XtremeIdiots.Portal.InvisionApiClient
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<InvisionApiClientOptions> options)
        {
            serviceCollection.Configure(options);

            serviceCollection.AddSingleton<ICoreApiClient, CoreApiClient>();
            serviceCollection.AddSingleton<IDownloadsApiClient, DownloadsApiClient>();
            serviceCollection.AddSingleton<IForumsApiClient, ForumsApiClient>();

            serviceCollection.AddSingleton<IInvisionApiClient, InvisionApiClient>();
        }
    }
}