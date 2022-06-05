using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient.Extensions;

namespace XtremeIdiots.Portal.RepositoryApiClient.Api
{
    public class GameServersStatsApi : BaseApi, IGameServersStatsApi
    {
        public GameServersStatsApi(ILogger<GameServersStatsApi> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<ApiResponseDto> CreateGameServerStats(List<CreateGameServerStatDto> createGameServerStatDtos)
        {
            var request = await CreateRequest($"game-servers-stats", Method.Post);
            request.AddJsonBody(createGameServerStatDtos);

            var response = await ExecuteAsync(request);

            return response.ToApiResponse();
        }

        public async Task<ApiResponseDto<GameServerStatCollectionDto>> GetGameServerStatusStats(Guid serverId, DateTime cutoff)
        {
            var request = await CreateRequest($"game-servers-stats/{serverId}", Method.Get);
            request.AddQueryParameter("cutoff", cutoff.ToString("MM/dd/yyyy HH:mm:ss"));

            var response = await ExecuteAsync(request);

            return response.ToApiResponse<GameServerStatCollectionDto>();
        }
    }
}
