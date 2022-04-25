using Azure.Security.KeyVault.Secrets;

namespace XtremeIdiots.Portal.RepositoryApiClient.GameServersSecretsApi;

public interface IGameServersSecretsApiClient
{
    Task<KeyVaultSecret?> GetGameServerSecret(string accessToken, string id, string secret);
    Task UpdateGameServerSecret(string accessToken, string id, string secret, string? secretValue);
}