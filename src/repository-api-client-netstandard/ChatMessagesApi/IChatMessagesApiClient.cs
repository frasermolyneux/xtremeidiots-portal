using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi
{
    public interface IChatMessagesApiClient
    {
        Task CreateChatMessage(string accessToken, ChatMessageApiDto chatMessage);
    }
}