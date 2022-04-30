using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.BanFileMonitorsApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public class RepositoryApiClient : IRepositoryApiClient
    {
        public RepositoryApiClient(
            IBanFileMonitorsApiClient banFileMonitorsApiClient,
            IChatMessagesApiClient chatMessagesApiClient,
            IGameServersApiClient gameServersApiClient,
            IPlayersApiClient playersApiClient)
        {
            BanFileMonitors = banFileMonitorsApiClient;
            ChatMessages = chatMessagesApiClient;
            GameServers = gameServersApiClient;
            Players = playersApiClient;
        }

        public IBanFileMonitorsApiClient BanFileMonitors { get; }
        public IChatMessagesApiClient ChatMessages { get; }
        public IGameServersApiClient GameServers { get; }
        public IPlayersApiClient Players { get; }
    }
}