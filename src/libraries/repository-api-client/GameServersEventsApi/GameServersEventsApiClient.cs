using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public class GameServersEventsApiClient : BaseApiClient, IGameServersEventsApiClient
{
    public GameServersEventsApiClient(ILogger<GameServersEventsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
    {
    }

    public async Task CreateGameServerEvent(string accessToken, string id, GameServerEventDto gameServerEvent)
    {
        var request = CreateRequest($"repository/game-servers/{id}/events", Method.Post, accessToken);
        request.AddJsonBody(new List<GameServerEventDto> { gameServerEvent });

        await ExecuteAsync(request);
    }
}