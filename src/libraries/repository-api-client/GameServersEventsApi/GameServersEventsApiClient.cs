using Microsoft.Extensions.Options;
using RestSharp;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public class GameServersEventsApiClient : BaseApiClient, IGameServersEventsApiClient
{
    public GameServersEventsApiClient(IOptions<RepositoryApiClientOptions> options) : base(options)
    {
    }

    public async Task CreateGameServerEvent(string accessToken, string id, GameServerEventDto gameServerEvent)
    {
        var request = CreateRequest($"repository/game-servers/{id}/events", Method.Post, accessToken);
        request.AddJsonBody(new List<GameServerEventDto> {gameServerEvent});

        await ExecuteAsync(request);
    }
}