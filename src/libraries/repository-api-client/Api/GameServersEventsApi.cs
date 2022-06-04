using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient.Interfaces;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api;

public class GameServersEventsApi : BaseApi, IGameServersEventsApi
{
    public GameServersEventsApi(ILogger<GameServersEventsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
    {
    }

    public async Task CreateGameServerEvent(GameServerEventDto gameServerEvent)
    {
        var request = await CreateRequest($"repository/game-servers/{gameServerEvent.GameServerId}/events", Method.Post);
        request.AddJsonBody(new List<GameServerEventDto> { gameServerEvent });

        await ExecuteAsync(request);
    }
}