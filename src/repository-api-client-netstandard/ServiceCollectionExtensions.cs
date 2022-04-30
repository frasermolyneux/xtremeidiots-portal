using Microsoft.Extensions.DependencyInjection;
using System;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
            Action<RepositoryApiClientOptions> configure)
        {
            serviceCollection.Configure(configure);

            serviceCollection.AddSingleton<IRepositoryTokenProvider, RepositoryTokenProvider>();

            serviceCollection.AddSingleton<IChatMessagesApiClient, ChatMessagesApiClient>();
            serviceCollection.AddSingleton<IPlayersApiClient, PlayersApiClient>();

            serviceCollection.AddSingleton<IRepositoryApiClient, RepositoryApiClient>();
        }
    }
}