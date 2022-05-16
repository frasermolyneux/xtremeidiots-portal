using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersEventsApi;

public class GameServersEventsApiClient : BaseApiClient, IGameServersEventsApiClient
{
    public GameServersEventsApiClient(ILogger<GameServersEventsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
    {
    }

    public async Task CreateGameServerEvent(GameServerEventDto gameServerEvent)
    {
        var request = await CreateRequest($"repository/game-servers/{gameServerEvent.GameServerId}/events", Method.Post);
        request.AddJsonBody(new List<GameServerEventDto> { gameServerEvent });

        await ExecuteAsync(request);
    }
}