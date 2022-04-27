using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XtremeIdiots.Portal.CommonLib.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class PlayersController : ControllerBase
{
    public PlayersController(ILogger<PlayersController> log, LegacyPortalContext context)
    {
        Log = log ?? throw new ArgumentNullException(nameof(log));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public LegacyPortalContext Context { get; }
    public ILogger<PlayersController> Log { get; }

    [HttpGet]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> GetPlayer(Guid playerId)
    {
        var player = await Context.Player2.SingleOrDefaultAsync(p => p.PlayerId == playerId);

        if (player == null) return new NotFoundResult();

        var playerDto = new PlayerApiDto
        {
            Id = player.PlayerId,
            GameType = player.GameType.ToString(),
            Username = player.Username,
            Guid = player.Guid,
            FirstSeen = player.FirstSeen,
            LastSeen = player.LastSeen,
            IpAddress = player.IpAddress
        };

        return new OkObjectResult(playerDto);
    }

    [HttpGet]
    [Route("api/players/by-game-type/{gameType}/{playerGuid}")]
    public async Task<IActionResult> GetPlayerByGameType(string gameType, string playerGuid)
    {
        GameType legacyGameType;
        switch (gameType)
        {
            case "CallOfDuty2":
                legacyGameType = GameType.CallOfDuty2;
                break;
            case "CallOfDuty4":
                legacyGameType = GameType.CallOfDuty4;
                break;
            case "CallOfDuty5":
                legacyGameType = GameType.CallOfDuty5;
                break;
            default:
                throw new Exception($"Unsupported game type {gameType}");
        }

        Log.LogInformation($"GetPlayerByGameType :: gameType: '{legacyGameType}', playerGuid: '{playerGuid}'");

        var player = await Context.Player2.SingleOrDefaultAsync(p => p.GameType == legacyGameType && p.Guid == playerGuid);

        if (player == null) return new NotFoundResult();

        var playerDto = new PlayerApiDto
        {
            Id = player.PlayerId,
            GameType = player.GameType.ToString(),
            Username = player.Username,
            Guid = player.Guid,
            FirstSeen = player.FirstSeen,
            LastSeen = player.LastSeen,
            IpAddress = player.IpAddress
        };

        Log.LogInformation($"GetPlayerByGameType :: gameType: '{legacyGameType}', playerGuid: '{playerGuid}' - Matched with '{playerDto.Id}'");

        return new OkObjectResult(playerDto);
    }

    [HttpPost]
    [Route("api/players")]
    public async Task<IActionResult> CreatePlayer()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<PlayerApiDto> players;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            players = JsonConvert.DeserializeObject<List<PlayerApiDto>>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (players == null) return new BadRequestResult();

        foreach (var player in players)
        {
            var existingPlayer =
                await Context.Player2.SingleOrDefaultAsync(p => p.GameType.ToString() == player.GameType && p.Guid == player.Guid);

            if (existingPlayer != null) return new ConflictObjectResult(existingPlayer);

            var player2 = new Player2
            {
                Username = player.Username.Trim(),
                Guid = player.Guid.ToLower().Trim(),

                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };

            if (IPAddress.TryParse(player.IpAddress, out var ip))
            {
                player2.IpAddress = ip.ToString();
            }

            await Context.Player2.AddAsync(player2);
        }

        await Context.SaveChangesAsync();

        return new OkObjectResult(players);
    }

    [HttpPatch]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> UpdatePlayer(Guid playerId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        PlayerApiDto player;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            player = JsonConvert.DeserializeObject<PlayerApiDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (player == null) return new BadRequestResult();
        if (player.Id != playerId) return new BadRequestResult();

        var playerToUpdate = await Context.Player2.SingleOrDefaultAsync(p => p.PlayerId == player.Id);

        if (playerToUpdate == null) return new NotFoundResult();

        if (!string.IsNullOrWhiteSpace(player.Username)) playerToUpdate.Username = player.Username.Trim();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (IPAddress.TryParse(playerToUpdate.IpAddress, out var ip))
        {
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            playerToUpdate.IpAddress = ip.ToString();
        }

        playerToUpdate.LastSeen = DateTime.UtcNow;

        await Context.SaveChangesAsync();

        var playerDto = new PlayerApiDto
        {
            Id = playerToUpdate.PlayerId,
            GameType = playerToUpdate.GameType.ToString(),
            Username = playerToUpdate.Username,
            Guid = playerToUpdate.Guid,
            FirstSeen = playerToUpdate.FirstSeen,
            LastSeen = playerToUpdate.LastSeen,
            IpAddress = playerToUpdate.IpAddress
        };

        return new OkObjectResult(playerDto);
    }
}