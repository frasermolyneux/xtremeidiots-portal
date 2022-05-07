using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi
{
    public interface IChatMessagesApiClient
    {
        Task<ChatMessageSearchEntryDto> GetChatMessage(string accessToken, Guid id);
        Task CreateChatMessage(string accessToken, ChatMessageDto chatMessage);
        Task<ChatMessageSearchResponseDto> SearchChatMessages(string accessToken, GameType? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order);
    }
}