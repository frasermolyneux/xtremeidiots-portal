using System.Net;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;

public class GameServersApiClient : BaseApiClient, IGameServersApiClient
{
    public GameServersApiClient(IOptions<RepositoryApiClientOptions> options) : base(options)
    {
    }

    public async Task<List<GameServerApiDto>?> GetGameServers(string accessToken)
    {
        var request = CreateRequest("repository/game-servers", Method.Get, accessToken);
        var response = await ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
            return JsonConvert.DeserializeObject<List<GameServerApiDto>>(response.Content);

        if (response.ErrorException != null)
            throw response.ErrorException;

        throw new Exception($"Failed to execute 'repository/game-servers' with '{response.StatusCode}'");
    }

    public async Task<GameServerApiDto?> GetGameServer(string accessToken, string id)
    {
        var request = CreateRequest($"repository/game-servers/{id}", Method.Get, accessToken);

        var response = await ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
            return JsonConvert.DeserializeObject<GameServerApiDto>(response.Content);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        throw new Exception($"Failed to execute 'repository/game-servers/{id}'");
    }

    public async Task CreateGameServer(string accessToken, GameServerApiDto gameServer)
    {
        var request = CreateRequest("repository/game-servers", Method.Post, accessToken);
        request.AddJsonBody(new List<GameServerApiDto> {gameServer});

        await ExecuteAsync(request);
    }

    public async Task UpdateGameServer(string accessToken, GameServerApiDto gameServer)
    {
        var request = CreateRequest($"repository/game-servers/{gameServer.Id}", Method.Patch, accessToken);
        request.AddJsonBody(gameServer);

        await ExecuteAsync(request);
    }
}