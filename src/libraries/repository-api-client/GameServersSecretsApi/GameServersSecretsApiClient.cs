using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersSecretsApi;

public class GameServersSecretsApiClient : BaseApiClient, IGameServersSecretsApiClient
{
    public GameServersSecretsApiClient(ILogger<GameServersSecretsApiClient> logger, IOptions<RepositoryApiClientOptions> options) : base(logger, options)
    {
    }

    public async Task<KeyVaultSecret?> GetGameServerSecret(string accessToken, string id, string secret)
    {
        var request = CreateRequest($"repository/game-servers/{id}/secrets/{secret}", Method.Get, accessToken);
        var response = await ExecuteAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.Content != null)
            return JsonConvert.DeserializeObject<KeyVaultSecret>(response.Content);
        else
            throw new Exception($"Response of {request.Method} to '{request.Resource}' has no content");
    }

    public async Task UpdateGameServerSecret(string accessToken, string id, string secret, string? secretValue)
    {
        var request = CreateRequest($"repository/game-servers/{id}/secrets/{secret}", Method.Post, accessToken);
        request.AddBody(secretValue ?? "", "text/plain");

        await ExecuteAsync(request);
    }
}