using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.RepositoryApiClient;

public class RepositoryApiTokenProvider : IRepositoryApiTokenProvider
{
    private readonly ILogger<RepositoryApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly IConfiguration configuration;

    public RepositoryApiTokenProvider(
        ILogger<RepositoryApiTokenProvider> logger,
        IMemoryCache memoryCache,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.configuration = configuration;
    }

    private string RepositoryApiApplicationAudience => configuration["repository-api-application-audience"];

    public async Task<string> GetAccessToken()
    {
        if (memoryCache.TryGetValue("repository-api-access-token", out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        var tokenCredential = new DefaultAzureCredential();

        try
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { $"{RepositoryApiApplicationAudience}/.default" }));
            memoryCache.Set("reository-api-access-token", accessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get identity token from AAD for audience: '{RepositoryApiApplicationAudience}'");
            throw;
        }

        return accessToken.Token;
    }
}