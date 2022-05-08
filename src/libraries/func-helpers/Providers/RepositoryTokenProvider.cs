using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.FuncHelpers.Providers;

public class RepositoryTokenProvider : IRepositoryTokenProvider
{
    public RepositoryTokenProvider(ILogger<RepositoryTokenProvider> log)
    {
        Log = log;
    }

    private ILogger<RepositoryTokenProvider> Log { get; }

    private string WebApiPortalApplicationAudience =>
        Environment.GetEnvironmentVariable("webapi-portal-application-audience");

    public async Task<string> GetRepositoryAccessToken()
    {
        var tokenCredential = new DefaultAzureCredential();

        AccessToken accessToken;
        try
        {
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{WebApiPortalApplicationAudience}/.default" }));
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to get managed identity token");
            throw;
        }

        return accessToken.Token;
    }
}