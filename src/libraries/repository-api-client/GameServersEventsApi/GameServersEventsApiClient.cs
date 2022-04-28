using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public class GameServersEventsApiClient : BaseApiClient, IGameServersEventsApiClient
{
    public GameServersEventsApiClient(ILogger<GameServersEventsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
    {
    }

    public async Task CreateGameServerEvent(string accessToken, string id, GameServerEventApiDto gameServerEvent)
    {
        var request = CreateRequest($"repository/game-servers/{id}/events", Method.Post, accessToken);
        request.AddJsonBody(new List<GameServerEventApiDto> { gameServerEvent });

        await ExecuteAsync(request);
    }
}