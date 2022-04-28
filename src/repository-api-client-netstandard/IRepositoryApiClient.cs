using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public interface IRepositoryApiClient
    {
        IChatMessagesApiClient ChatMessages { get; }
    }
}