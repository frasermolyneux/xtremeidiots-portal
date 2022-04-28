using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersApi;

public class GameServersApiClient : BaseApiClient, IGameServersApiClient
{
    public GameServersApiClient(ILogger<GameServersApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
    {
    }

    public async Task<List<GameServerApiDto>?> GetGameServers(string accessToken)
    {
        var request = CreateRequest("repository/game-servers", Method.Get, accessToken);
        var response = await ExecuteAsync(request);

        if (response.Content != null)
            return JsonConvert.DeserializeObject<List<GameServerApiDto>>(response.Content);
        else
            throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
    }

    public async Task<GameServerApiDto?> GetGameServer(string accessToken, string id)
    {
        var request = CreateRequest($"repository/game-servers/{id}", Method.Get, accessToken);
        var response = await ExecuteAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.Content != null)
            return JsonConvert.DeserializeObject<GameServerApiDto>(response.Content);
        else
            throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
    }

    public async Task CreateGameServer(string accessToken, GameServerApiDto gameServer)
    {
        var request = CreateRequest("repository/game-servers", Method.Post, accessToken);
        request.AddJsonBody(new List<GameServerApiDto> { gameServer });

        await ExecuteAsync(request);
    }

    public async Task UpdateGameServer(string accessToken, GameServerApiDto gameServer)
    {
        var request = CreateRequest($"repository/game-servers/{gameServer.Id}", Method.Patch, accessToken);
        request.AddJsonBody(gameServer);

        await ExecuteAsync(request);
    }
}