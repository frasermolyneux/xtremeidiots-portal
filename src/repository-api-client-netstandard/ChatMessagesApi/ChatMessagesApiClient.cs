using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi
{
    public class ChatMessagesApiClient : BaseApiClient, IChatMessagesApiClient
    {
        public ChatMessagesApiClient(ILogger<ChatMessagesApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {

        }

        public async Task<ChatMessageSearchEntryDto> GetChatMessage(string accessToken, Guid id)
        {
            var request = CreateRequest($"repository/chat-messages/{id}", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<ChatMessageSearchEntryDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task CreateChatMessage(string accessToken, ChatMessageApiDto chatMessage)
        {
            var request = CreateRequest("repository/chat-messages", Method.POST, accessToken);
            request.AddJsonBody(new List<ChatMessageApiDto> { chatMessage });

            await ExecuteAsync(request);
        }

        public async Task<ChatMessageSearchResponseDto> SearchChatMessages(string accessToken, string? gameType, Guid? serverId, Guid? playerId, string filterString, int takeEntries, int skipEntries, string? order)
        {
            var request = CreateRequest("repository/chat-messages/search", Method.GET, accessToken);

            if (!string.IsNullOrEmpty(gameType))
                request.AddQueryParameter("gameType", gameType);

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