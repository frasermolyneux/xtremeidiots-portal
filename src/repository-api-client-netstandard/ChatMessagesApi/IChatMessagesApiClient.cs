using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi
{
    public interface IChatMessagesApiClient
    {
        Task<ChatMessageSearchEntryDto> GetChatMessage(Guid id);
        Task CreateChatMessage(ChatMessageDto chatMessage);
        Task<ChatMessageSearchResponseDto> SearchChatMessages(GameType? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order);
    }
}