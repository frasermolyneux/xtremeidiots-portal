using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XtremeIdiots.Portal.DataLib;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class GameServersSecretsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public GameServersSecretsController(PortalDbContext context, IConfiguration configuration)
    {
        _configuration = configuration;
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public PortalDbContext Context { get; }

    [HttpGet]
    [Route("repository/game-servers/{serverId}/secret/{secretId}")]
    public async Task<IActionResult> GetGameServerSecret(string serverId, string secretId)
    {
        if (string.IsNullOrWhiteSpace(serverId)) return new BadRequestResult();

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId.ToString() == serverId);
        if (gameServer == null) return new BadRequestResult();

        var client = new SecretClient(new Uri(_configuration["gameservers-keyvault-endpoint"]),
            new DefaultAzureCredential());

        try
        {
            var keyVaultResponse = await client.GetSecretAsync($"{serverId}-{secretId}");
            return new OkObjectResult(keyVaultResponse.Value);
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == 404)
                return new NotFoundResult();

            throw;
        }
    }

    [HttpPost]
    [Route("repository/game-servers/{serverId}/secret/{secretId}")]
    public async Task<IActionResult> SetGameServerSecret(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(id)) return new BadRequestResult();

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId.ToString() == id);
        if (gameServer == null) return new BadRequestResult();

        var client = new SecretClient(new Uri(_configuration["gameservers-keyvault-endpoint"]),
            new DefaultAzureCredential());

        var rawSecretValue = await new StreamReader(Request.Body).ReadToEndAsync();

        try
        {
            var keyVaultResponse = await client.GetSecretAsync($"{id}-{name}");

            if (keyVaultResponse.Value.Value != rawSecretValue)
                keyVaultResponse = await client.SetSecretAsync($"{id}-{name}", rawSecretValue);

            return new OkObjectResult(keyVaultResponse.Value);
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status != 404)
                throw;
        }

        var newSecretKeyVaultResponse = await client.SetSecretAsync($"{id}-{name}", rawSecretValue);
        return new OkObjectResult(newSecretKeyVaultResponse.Value);
    }
}