using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.ChatMessagesApi;

public class ChatMessagesApiClient : BaseApiClient, IChatMessagesApiClient
{
    public ChatMessagesApiClient(ILogger<ChatMessagesApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
    {

    }

    public async Task CreateChatMessage(string accessToken, ChatMessageApiDto chatMessage)
    {
        var request = CreateRequest("repository/chat-messages", Method.Post, accessToken);
        request.AddJsonBody(new List<ChatMessageApiDto> { chatMessage });

        await ExecuteAsync(request);
    }
}