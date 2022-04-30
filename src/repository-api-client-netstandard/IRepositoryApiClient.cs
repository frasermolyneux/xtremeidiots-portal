using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public interface IRepositoryApiClient
    {
        IChatMessagesApiClient ChatMessages { get; }
        IPlayersApiClient PlayersApiClient { get; }
    }
}