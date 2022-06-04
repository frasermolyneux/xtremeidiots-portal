using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages;

namespace XtremeIdiots.Portal.RepositoryApiClient.Interfaces
{
    public interface IChatMessagesApi
    {
        Task<ChatMessageSearchEntryDto?> GetChatMessage(Guid id);
        Task CreateChatMessage(ChatMessageDto chatMessage);
        Task<ChatMessageSearchResponseDto?> SearchChatMessages(GameType? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order);
    }
}