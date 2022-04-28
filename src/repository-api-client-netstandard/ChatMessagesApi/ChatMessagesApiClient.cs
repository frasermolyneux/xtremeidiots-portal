using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.ChatMessagesApi
{
    public class ChatMessagesApiClient : BaseApiClient, IChatMessagesApiClient
    {
        public ChatMessagesApiClient(ILogger<ChatMessagesApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {

        }

        public async Task CreateChatMessage(string accessToken, ChatMessageApiDto chatMessage)
        {
            var request = CreateRequest("repository/chat-messages", Method.POST, accessToken);
            request.AddJsonBody(new List<ChatMessageApiDto> { chatMessage });

            await ExecuteAsync(request);
        }
    }
}