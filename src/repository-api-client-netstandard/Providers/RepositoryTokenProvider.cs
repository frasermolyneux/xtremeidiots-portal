using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace XtremeIdiots.Portal.FuncHelpers.Providers
{
    public class RepositoryTokenProvider : IRepositoryTokenProvider
    {
        public RepositoryTokenProvider(ILogger log)
        {
            Log = log;
        }

        private ILogger Log { get; }

        public async Task<string> GetRepositoryAccessToken()
        {
            var tokenCredential = new ManagedIdentityCredential();

            AccessToken accessToken;
            try
            {
                // Go away - I know this is hardcoded but this a temp hack
                accessToken = await tokenCredential.GetTokenAsync(
                    new TokenRequestContext(new[] { $"api://portal-repository-api-prd" }));
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