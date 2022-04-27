using System.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.PlayersApi;

public class PlayersApiClient : BaseApiClient, IPlayersApiClient
{
    public PlayersApiClient(IOptions<RepositoryApiClientOptions> options) : base(options)
    {
    }

    public async Task<PlayerApiDto?> GetPlayer(string accessToken, Guid id)
    {
        var request = CreateRequest($"repository/players/{id}", Method.Get, accessToken);
        var response = await ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
            return JsonConvert.DeserializeObject<PlayerApiDto>(response.Content);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        throw new Exception("Failed to execute 'repository/Player'");
    }

    public async Task<PlayerApiDto?> GetPlayerByGameType(string accessToken, string gameType, string guid)
    {
        var request = CreateRequest($"repository/players/by-game-type/{gameType}/{guid}", Method.Get, accessToken);

        var response = await ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
            return JsonConvert.DeserializeObject<PlayerApiDto>(response.Content);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        throw new Exception($"Failed to execute 'repository/players/by-game-type/{gameType}/{guid}'");
    }

    public async Task CreatePlayer(string accessToken, PlayerApiDto player)
    {
        var request = CreateRequest("repository/players", Method.Post, accessToken);
        request.AddJsonBody(new List<PlayerApiDto> {player});

        await ExecuteAsync(request);
    }

    public async Task UpdatePlayer(string accessToken, PlayerApiDto player)
    {
        var request = CreateRequest($"repository/players/{player.Id}", Method.Patch, accessToken);
        request.AddJsonBody(player);

        await ExecuteAsync(request);
    }
}