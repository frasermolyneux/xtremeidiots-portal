using System.Net;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersSecretsApi;

public class GameServersSecretsApiClient : BaseApiClient, IGameServersSecretsApiClient
{
    public GameServersSecretsApiClient(IOptions<RepositoryApiClientOptions> options) : base(options)
    {
    }

    public async Task<KeyVaultSecret?> GetGameServerSecret(string accessToken, string id, string secret)
    {
        var request = CreateRequest($"repository/game-servers/{id}/secrets/{secret}", Method.Get, accessToken);
        var response = await ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
            return JsonConvert.DeserializeObject<KeyVaultSecret>(response.Content);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        throw new Exception($"Failed to execute 'repository/game-servers/{id}/secrets/{secret}'");
    }

    public async Task UpdateGameServerSecret(string accessToken, string id, string secret, string? secretValue)
    {
        var request = CreateRequest($"repository/game-servers/{id}/secrets/{secret}", Method.Post, accessToken);
        request.AddBody(secretValue ?? "", "text/plain");

        await ExecuteAsync(request);
    }
}