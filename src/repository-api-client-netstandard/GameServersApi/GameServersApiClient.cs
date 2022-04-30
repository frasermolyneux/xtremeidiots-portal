using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.GameServersApi
{
    public class GameServersApiClient : BaseApiClient, IGameServersApiClient
    {
        public GameServersApiClient(ILogger<GameServersApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<List<GameServerDto>?> GetGameServers(string accessToken, string[] gameTypes, Guid[] serverIds, string filterOption, int skipEntries, int takeEntries, string order)
        {
            var request = CreateRequest("repository/game-servers", Method.GET, accessToken);

            if (gameTypes != null)
                request.AddQueryParameter("gameTypes", string.Join(",", gameTypes));

            if (serverIds != null)
                request.AddQueryParameter("serverIds", string.Join(",", serverIds));

            if (!string.IsNullOrEmpty(filterOption))
                request.AddQueryParameter("filterOption", filterOption);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<GameServerDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<GameServerDto?> GetGameServer(string accessToken, Guid serverId)
        {
            var request = CreateRequest($"repository/game-servers/{serverId}", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<GameServerDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task DeleteGameServer(string accessToken, Guid id)
        {
            var request = CreateRequest($"repository/game-servers/{id}", Method.DELETE, accessToken);
            await ExecuteAsync(request);
        }

        public async Task CreateGameServer(string accessToken, GameServerDto gameServer)
        {
            var request = CreateRequest("repository/game-servers", Method.POST, accessToken);
            request.AddJsonBody(new List<GameServerDto> { gameServer });

            await ExecuteAsync(request);
        }

        public async Task UpdateGameServer(string accessToken, GameServerDto gameServer)
        {
            var request = CreateRequest($"repository/game-servers/{gameServer.Id}", Method.PATCH, accessToken);
            request.AddJsonBody(gameServer);

            await ExecuteAsync(request);
        }

        public async Task<BanFileMonitorDto> CreateBanFileMonitorForGameServer(string accessToken, Guid serverId, BanFileMonitorDto banFileMonitor)
        {
            var request = CreateRequest($"repository/game-servers/{serverId}/ban-file-monitors", Method.POST, accessToken);
            request.AddJsonBody(banFileMonitor);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<BanFileMonitorDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}