using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.PlayersApi
{
    public class PlayersApiClient : BaseApiClient, IPlayersApiClient
    {
        public PlayersApiClient(ILogger<PlayersApiClient> logger, IOptions<RepositoryApiClientOptions> options, IRepositoryApiTokenProvider repositoryApiTokenProvider) : base(logger, options, repositoryApiTokenProvider)
        {
        }

        public async Task<PlayerDto?> GetPlayer(Guid id)
        {
            var request = await CreateRequest($"repository/players/{id}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<PlayerDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<AliasDto>> GetPlayerAliases(Guid id)
        {
            var request = await CreateRequest($"repository/players/{id}/aliases", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<AliasDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<IpAddressDto>> GetPlayerIpAddresses(Guid id)
        {
            var request = await CreateRequest($"repository/players/{id}/ip-addresses", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<IpAddressDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<RelatedPlayerDto>> GetRelatedPlayers(Guid id, string ipAddress)
        {
            var request = await CreateRequest($"repository/players/{id}/related-players", Method.Get);
            request.AddQueryParameter("IpAddress", ipAddress);

            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<RelatedPlayerDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<PlayerDto?> GetPlayerByGameType(GameType gameType, string guid)
        {
            var request = await CreateRequest($"repository/players/by-game-type/{gameType}/{guid}", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.Content != null)
                return JsonConvert.DeserializeObject<PlayerDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task CreatePlayer(PlayerDto player)
        {
            var request = await CreateRequest("repository/players", Method.Post);
            request.AddJsonBody(new List<PlayerDto> { player });

            await ExecuteAsync(request);
        }

        public async Task UpdatePlayer(PlayerDto player)
        {
            var request = await CreateRequest($"repository/players/{player.Id}", Method.Patch);
            request.AddJsonBody(player);

            await ExecuteAsync(request);
        }

        public async Task<PlayersSearchResponseDto> SearchPlayers(string gameType, string filterType, string filterString, int takeEntries, int skipEntries, string? order)
        {
            var request = await CreateRequest("repository/players/search", Method.Get);

            if (!string.IsNullOrWhiteSpace(gameType))
                request.AddQueryParameter("gameType", gameType);

            if (!string.IsNullOrWhiteSpace(filterType))
                request.AddQueryParameter("filterType", filterType);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            if (!string.IsNullOrWhiteSpace(filterString))
                request.AddQueryParameter("filterString", filterString);

            request.AddQueryParameter("takeEntries", takeEntries.ToString());
            request.AddQueryParameter("skipEntries", skipEntries.ToString());

            if (!string.IsNullOrWhiteSpace(order))
                request.AddQueryParameter("order", order);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<PlayersSearchResponseDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<List<AdminActionDto>> GetAdminActionsForPlayer(Guid playerId)
        {
            var request = await CreateRequest($"repository/players/{playerId}/admin-actions", Method.Get);
            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<List<AdminActionDto>>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<AdminActionDto> CreateAdminActionForPlayer(AdminActionDto adminAction)
        {
            var request = await CreateRequest($"repository/players/{adminAction.PlayerId}/admin-actions", Method.Post);
            request.AddJsonBody(adminAction);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<AdminActionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }

        public async Task<AdminActionDto> UpdateAdminActionForPlayer(AdminActionDto adminAction)
        {
            var request = await CreateRequest($"repository/players/{adminAction.PlayerId}/admin-actions/{adminAction.AdminActionId}", Method.Patch);
            request.AddJsonBody(adminAction);

            var response = await ExecuteAsync(request);

            if (response.Content != null)
                return JsonConvert.DeserializeObject<AdminActionDto>(response.Content);
            else
                throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
        }
    }
}