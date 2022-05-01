using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers
{
    public class RepositoryTokenProvider : IRepositoryTokenProvider
    {
        public RepositoryTokenProvider(ILogger<RepositoryTokenProvider> log)
        {
            Log = log;
        }

        private ILogger<RepositoryTokenProvider> Log { get; }

        public async Task<string> GetRepositoryAccessToken()
        {
            var tokenCredential = new DefaultAzureCredential();

            AccessToken accessToken;
            try
            {
                // Go away - I know this is hardcoded but this a temp hack
                accessToken = await tokenCredential.GetTokenAsync(
                    new TokenRequestContext(new[] { $"api://portal-repository-api-prd/.default" }));
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Failed to get managed identity token");
                throw;
            }

            return accessToken.Token;
        }
    }
}