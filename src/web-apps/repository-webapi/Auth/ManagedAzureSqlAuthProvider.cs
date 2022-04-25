using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace XtremeIdiots.Portal.RepositoryWebApi.Auth;

//https://docs.microsoft.com/en-us/azure/app-service/tutorial-connect-msi-sql-database?tabs=windowsclient%2Cdotnetcore#modify-your-project
public class ManagedAzureSqlAuthProvider : SqlAuthenticationProvider
{
    private static readonly string[] AzureSqlScopes =
    {
        "https://database.windows.net/.default"
    };

    private static readonly TokenCredential Credential = new ManagedIdentityCredential();

    public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
    {
        var tokenRequestContext = new TokenRequestContext(AzureSqlScopes);
        var tokenResult = await Credential.GetTokenAsync(tokenRequestContext, default);
        return new SqlAuthenticationToken(tokenResult.Token, tokenResult.ExpiresOn);
    }

    public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
    {
        return authenticationMethod.Equals(SqlAuthenticationMethod.ActiveDirectoryDeviceCodeFlow);
    }
}