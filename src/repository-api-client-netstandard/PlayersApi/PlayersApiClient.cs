using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi
{
    public class PlayersApiClient : BaseApiClient, IPlayersApiClient
    {
        public PlayersApiClient(ILogger<PlayersApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
        {
        }

        public async Task<PlayerDto?> GetPlayer(string accessToken, Guid id)
        {
            var request = CreateRequest($"repository/players/{id}", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<PlayerDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<AliasDto>> GetPlayerAliases(string accessToken, Guid id)
        {
            var request = CreateRequest($"repository/players/{id}/aliases", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<AliasDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<IpAddressDto>> GetPlayerIpAddresses(string accessToken, Guid id)
        {
            var request = CreateRequest($"repository/players/{id}/ip-addresses", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<IpAddressDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<RelatedPlayerDto>> GetRelatedPlayers(string accessToken, Guid id, string ipAddress)
        {
            var request = CreateRequest($"repository/players/{id}/related-players", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<RelatedPlayerDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<PlayerDto?> GetPlayerByGameType(string accessToken, string gameType, string guid)
        {
            var request = CreateRequest($"repository/players/by-game-type/{gameType}/{guid}", Method.GET, accessToken);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<PlayerDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task CreatePlayer(string accessToken, PlayerDto player)
        {
            var request = CreateRequest("repository/players", Method.POST, accessToken);
            request.AddJsonBody(new List<PlayerDto> { player });

            await ExecuteAsync(request);
        }

        public async Task UpdatePlayer(string accessToken, PlayerDto player)
        {
            var request = CreateRequest($"repository/players/{player.Id}", Method.PATCH, accessToken);
            request.AddJsonBody(player);

            await ExecuteAsync(request);
        }
    }
}