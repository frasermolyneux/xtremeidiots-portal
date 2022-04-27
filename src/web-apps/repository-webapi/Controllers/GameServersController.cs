using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using XI.Portal.Data.Legacy;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class GameServersController : Controller
{
    public GameServersController(LegacyPortalContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public LegacyPortalContext Context { get; }

    [HttpGet]
    [Route("api/game-servers")]
    public async Task<IActionResult> GetGameServer()
    {
        var gameServers = await Context.GameServers.ToListAsync();

        return new OkObjectResult(gameServers);
    }

    [HttpGet]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> GetGameServer(string serverId)
    {
        if (string.IsNullOrWhiteSpace(serverId)) return new BadRequestResult();

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId.ToString() == serverId);

        if (gameServer == null) return new NotFoundResult();

        return new OkObjectResult(gameServer);
    }

    [HttpPost]
    [Route("api/game-servers")]
    public async Task<IActionResult> CreateGameServer()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<GameServerDto> gameServers;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            gameServers = JsonConvert.DeserializeObject<List<GameServerDto>>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (gameServers == null) return new BadRequestResult();

        foreach (var gameServer in gameServers)
        {
            var existingGameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId.ToString() == gameServer.Id);
            if (existingGameServer != null) return new ConflictObjectResult(existingGameServer);

            if (string.IsNullOrWhiteSpace(gameServer.Title)) gameServer.Title = "to-be-updated";

            if (string.IsNullOrWhiteSpace(gameServer.IpAddress)) gameServer.IpAddress = "127.0.0.1";

            //await Context.GameServers.AddAsync(gameServer);
        }

        //await Context.SaveChangesAsync();

        return new OkObjectResult(gameServers);
    }

    [HttpPatch]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> UpdateGameServer(string serverId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        GameServerDto gameServer;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            gameServer = JsonConvert.DeserializeObject<GameServerDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (gameServer == null) return new BadRequestResult();

        if (gameServer.Id != serverId) return new BadRequestResult();

        var gameServerToUpdate = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId.ToString() == serverId);

        if (gameServerToUpdate == null) return new NotFoundResult();

        if (!string.IsNullOrWhiteSpace(gameServer.Title)) gameServerToUpdate.Title = gameServer.Title.Trim();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (IPAddress.TryParse(gameServerToUpdate.Hostname, out var ip))
        {
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            gameServerToUpdate.Hostname = ip.ToString();
        }

        if (gameServerToUpdate.QueryPort != 0) gameServerToUpdate.QueryPort = gameServer.QueryPort;

        await Context.SaveChangesAsync();

        return new OkObjectResult(gameServerToUpdate);
    }
}