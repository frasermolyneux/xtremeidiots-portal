using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard
{
    public class RepositoryApiClient : IRepositoryApiClient
    {
        public RepositoryApiClient(
            IChatMessagesApiClient chatMessagesApiClient)
        {
            ChatMessages = chatMessagesApiClient;
        }

        public IChatMessagesApiClient ChatMessages { get; }
    }
}