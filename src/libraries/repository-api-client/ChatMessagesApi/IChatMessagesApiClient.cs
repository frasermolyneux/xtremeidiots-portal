using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;

public interface IChatMessagesApiClient
{
    Task CreateChatMessage(string accessToken, ChatMessageDto chatMessage);
}