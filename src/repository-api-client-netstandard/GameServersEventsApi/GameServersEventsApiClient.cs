using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersEventsApi
{
    public class GameServersEventsApiClient : BaseApiClient, IGameServersEventsApiClient
    {
        public GameServersEventsApiClient(ILogger<GameServersEventsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task CreateGameServerEvent(string accessToken, GameServerEventDto gameServerEvent)
        {
            var request = CreateRequest($"repository/game-servers/{gameServerEvent.GameServerId}/events", Method.POST, accessToken);
            request.AddJsonBody(new List<GameServerEventDto> { gameServerEvent });

            await ExecuteAsync(request);
        }
    }
}