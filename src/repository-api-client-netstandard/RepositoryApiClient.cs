using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public class RepositoryApiClient : IRepositoryApiClient
    {
        public RepositoryApiClient(
            IChatMessagesApiClient chatMessagesApiClient,
            IPlayersApiClient playersApiClient)
        {
            ChatMessages = chatMessagesApiClient;
            PlayersApiClient = playersApiClient;
        }

        public IChatMessagesApiClient ChatMessages { get; }
        public IPlayersApiClient PlayersApiClient { get; }
    }
}