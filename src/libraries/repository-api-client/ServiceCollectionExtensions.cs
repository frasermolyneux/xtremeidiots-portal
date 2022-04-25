using Microsoft.Extensions.DependencyInjection;
using XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.DataMaintenanceApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;
using XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient;

public static class ServiceCollectionExtensions
{
    public static void AddRepositoryApiClient(this IServiceCollection serviceCollection,
        Action<RepositoryApiClientOptions> configure)
    {
        serviceCollection.Configure(configure);

        serviceCollection.AddSingleton<IChatMessagesApiClient, ChatMessagesApiClient>();
        serviceCollection.AddSingleton<IDataMaintenanceApiClient, DataMaintenanceApiClient>();
        serviceCollection.AddSingleton<IGameServersApiClient, GameServersApiClient>();
        serviceCollection.AddSingleton<IGameServersEventsApiClient, GameServersEventsApiClient>();
        serviceCollection.AddSingleton<IGameServersEventsApiClient, GameServersEventsApiClient>();
        serviceCollection.AddSingleton<IPlayersApiClient, PlayersApiClient>();

        serviceCollection.AddSingleton<IRepositoryApiClient, RepositoryApiClient>();
    }
}