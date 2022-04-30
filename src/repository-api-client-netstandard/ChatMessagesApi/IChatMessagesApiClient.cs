using System;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi
{
    public interface IChatMessagesApiClient
    {
        Task<ChatMessageSearchEntryDto> GetChatMessage(string accessToken, Guid id);
        Task CreateChatMessage(string accessToken, ChatMessageApiDto chatMessage);
        Task<ChatMessageSearchResponseDto> SearchChatMessages(string accessToken, string? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order);
    }
}