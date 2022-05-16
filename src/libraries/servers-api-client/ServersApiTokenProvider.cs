using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.FuncHelpers.Providers;

public class ServersApiTokenProvider : IServersApiTokenProvider
{
    public ServersApiTokenProvider(
        ILogger<ServersApiTokenProvider> log,
        IConfiguration configuration)
    {
        Log = log;
        Configuration = configuration;
    }

    private ILogger<ServersApiTokenProvider> Log { get; }
    public IConfiguration Configuration { get; }

    private string ServersApiApplicationAudience => Configuration["servers-api-application-audience"];

    public async Task<string> GetAccessToken()
    {
        var tokenCredential = new DefaultAzureCredential();

        AccessToken accessToken;
        try
        {
            accessToken = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(new[] { $"{ServersApiApplicationAudience}/.default" }));
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to get identity token");
            throw;
        }

        return accessToken.Token;
    }
}