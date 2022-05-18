using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi
{
    public interface IChatMessagesApiClient
    {
        Task<ChatMessageSearchEntryDto?> GetChatMessage(Guid id);
        Task CreateChatMessage(ChatMessageDto chatMessage);
        Task<ChatMessageSearchResponseDto?> SearchChatMessages(GameType? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order);
    }
}