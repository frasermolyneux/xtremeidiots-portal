using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.ServersApiClient;

public class ServersApiTokenProvider : IServersApiTokenProvider
{
    private readonly ILogger<ServersApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly IConfiguration configuration;

    public ServersApiTokenProvider(
        ILogger<ServersApiTokenProvider> logger,
        IMemoryCache memoryCache,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.configuration = configuration;
    }

    private string ServersApiApplicationAudience => configuration["servers_api_application_audience"];

    public async Task<string> GetAccessToken()
    {
        if (memoryCache.TryGetValue("servers-api-access-token", out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        var tokenCredential = new DefaultAzureCredential();

        try
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { $"{ServersApiApplicationAudience}/.default" }));
            memoryCache.Set("servers-api-access-token", accessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get identity token from AAD for audience: '{ServersApiApplicationAudience}'");
            throw;
        }

        return accessToken.Token;
    }
}