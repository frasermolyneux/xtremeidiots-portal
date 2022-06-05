using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RestSharp;

using System.Net;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.ChatMessages;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class ChatMessagesApi : BaseApi, IChatMessagesApi
    {
        public ChatMessagesApi(ILogger<ChatMessagesApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {

        }

        public async Task<ChatMessageSearchEntryDto?> GetChatMessage(Guid id)
        {
            var request = await CreateRequest($"chat-messages/{id}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ChatMessageSearchEntryDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task CreateChatMessage(ChatMessageDto chatMessage)
        {
            var request = await CreateRequest("chat-messages", Method.Post);
            request.AddJsonBody(new List<ChatMessageDto> { chatMessage });

            await ExecuteAsync(request);
        }

        public async Task<ChatMessageSearchResponseDto?> SearchChatMessages(GameType? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order)
        {
            var request = await CreateRequest("chat-messages/search", Method.Get);

            if (gameType != null)
                request.AddQueryParameter("gameType", gameType.ToString());

            if (serverId != null)
                request.AddQueryParameter("serverId", serverId.ToString());

            if (playerId != null)
                request.AddQueryParameter("playerId", playerId.ToString());

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ChatMessageSearchResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}