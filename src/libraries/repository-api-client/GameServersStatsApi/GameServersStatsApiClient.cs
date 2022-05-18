﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersStatsApi
{
    public class GameServersStatsApiClient : BaseApiClient, IGameServersStatsApiClient
    {
        public GameServersStatsApiClient(ILogger<GameServersStatsApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<List<GameServerStatDto>?> CreateGameServerStats(List<GameServerStatDto> gameServerStatDtos)
        {
            var request = await CreateRequest($"repository/game-servers-stats", Method.Post);
            request.AddJsonBody(gameServerStatDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<GameServerStatDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}