using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;

public interface IChatMessagesApiClient
{
    Task CreateChatMessage(string accessToken, ChatMessageDto chatMessage);
}