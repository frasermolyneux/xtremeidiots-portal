﻿using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace XtremeIdiots.Portal.FuncHelpers.Providers;

public class RepositoryTokenProvider : IRepositoryTokenProvider
{
    public RepositoryTokenProvider(
        ILogger<RepositoryTokenProvider> log,
        IConfiguration configuration)
    {
        Log = log;
        Configuration = configuration;
    }

    private ILogger<RepositoryTokenProvider> Log { get; }
    public IConfiguration Configuration { get; }

    private string WebApiPortalApplicationAudience => Configuration["repository-api-application-audience"];

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