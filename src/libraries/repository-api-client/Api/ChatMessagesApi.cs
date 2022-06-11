using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class ChatMessagesApi : BaseApi, IChatMessagesApi
    {
        public ChatMessagesApi(ILogger<ChatMessagesApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {

        }

        public async Task<ApiResponseDto<ChatMessageDto>> GetChatMessage(Guid chatMessageId)
        {
            var request = await CreateRequest($"chat-messages/{chatMessageId}", Method.Get);
            var response = await ExecuteAsync(request);

            return response.ToApiResponse<ChatMessageDto>();
        }

        public async Task<ApiResponseDto<ChatMessagesCollectionDto>> GetChatMessages(GameType? gameType, Guid? serverId, Guid? playerId, string filterString, int skipEntries, int takeEntries, ChatMessageOrder? order)
        {
            var request = await CreateRequest("chat-messages", Method.Get);

            if (gameType.HasValue)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (serverId.HasValue)
                request.AddQueryParameter("serverId", serverId.ToString());

            if (playerId.HasValue)
                request.AddQueryParameter("playerId", playerId.ToString());

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (order.HasValue)
                request.AddQueryParameter("order", order.ToString());

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<ChatMessagesCollectionDto>();
        }

        public async Task<ApiResponseDto> CreateChatMessage(CreateChatMessageDto createChatMessageDto)
        {
            var request = await CreateRequest("chat-messages", Method.Post);
            request.AddJsonBody(new List<CreateChatMessageDto> { createChatMessageDto });

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto> CreateChatMessages(List<CreateChatMessageDto> createChatMessageDtos)
        {
            var request = await CreateRequest("chat-messages", Method.Post);
            request.AddJsonBody(createChatMessageDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }
    }
}