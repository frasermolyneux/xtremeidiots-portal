﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.LivePlayersApi
{
    public class LivePlayersApiClient : BaseApiClient, ILivePlayersApiClient
    {
        public LivePlayersApiClient(ILogger<LivePlayersApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<List<LivePlayerDto>?> CreateGameServerLivePlayers(Guid serverId, List<LivePlayerDto> livePlayerDtos)
        {
            var request = await CreateRequest($"repository/live-players/{serverId}", Method.Post);
            request.AddJsonBody(livePlayerDtos);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<List<LivePlayerDto>>(response.Content);
                return result ?? throw new Exception($"Response of {request.Method} to '{request.Resource}' has no entities");
            }
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}