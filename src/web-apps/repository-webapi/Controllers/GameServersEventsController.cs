using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class GameServersEventsController : ControllerBase
{
    private readonly ILogger<GameServersEventsController> logger;
    private readonly PortalDbContext context;

    public GameServersEventsController(
        ILogger<GameServersEventsController> logger,
        PortalDbContext context)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpPost]
    [Route("api/game-servers/{serverId}/event")]
    public async Task<IActionResult> CreateGameServerEvent(Guid serverId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        GameServerEventDto gameServerEvent;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            gameServerEvent = JsonConvert.DeserializeObject<GameServerEventDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (gameServerEvent == null) return new BadRequestResult();
        if (gameServerEvent.GameServerId != serverId) return new BadRequestResult();

        //await Context.GameServerEvents.AddAsync(gameServerEvent);
        //await Context.SaveChangesAsync();

        return new OkObjectResult(gameServerEvent);
    }
}