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

        List<PlayerApiDto> playerDtos;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            playerDtos = JsonConvert.DeserializeObject<List<PlayerApiDto>>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (playerDtos == null) return new BadRequestResult();

        foreach (var player in playerDtos)
        {
            GameType legacyGameType;
            switch (player.GameType)
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
                    throw new Exception($"Unsupported game type {player.GameType}");
            }

            var existingPlayer =
                await Context.Player2.SingleOrDefaultAsync(p => p.GameType == legacyGameType && p.Guid == player.Guid);

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

        return new OkObjectResult(playerDtos);
    }

    [HttpPatch]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> UpdatePlayer(Guid playerId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        PlayerApiDto playerDto;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            playerDto = JsonConvert.DeserializeObject<PlayerApiDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }

        if (playerDto == null) return new BadRequestResult();
        if (playerDto.Id != playerId) return new BadRequestResult();

        playerDto.Username = playerDto.Username.Trim();

        var player = await Context.Player2
                .Include(p => p.PlayerAlias)
                .Include(p => p.PlayerIpAddresses)
                .SingleOrDefaultAsync(p => p.PlayerId == playerDto.Id);

        if (player == null) return new NotFoundResult();

        bool entityWorthUpdating = false;
        if (!string.IsNullOrWhiteSpace(playerDto.Username) && playerDto.Username != player.Username)
            entityWorthUpdating = true;

        if (IPAddress.TryParse(player.IpAddress, out var ip) && playerDto.IpAddress != player.IpAddress)
            entityWorthUpdating = true;

        if (DateTime.UtcNow - player.LastSeen > TimeSpan.FromMinutes(5))
            entityWorthUpdating = true;

        if (entityWorthUpdating)
        {
            if (player.Username != playerDto.Username)
            {
                if (player.PlayerAlias.Any(a => a.Name == player.Username))
                {
                    var existingUsernameAlias = player.PlayerAlias.First(a => a.Name == player.Username);
                    existingUsernameAlias.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    Context.PlayerAlias.Add(new PlayerAlias
                    {
                        PlayerAliasId = Guid.NewGuid(),
                        Name = player.Username,
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow,
                        PlayerPlayer = player
                    });
                }

                if (player.PlayerAlias.Any(a => a.Name == playerDto.Username))
                {
                    var existingNewUsernameAlias = player.PlayerAlias.First(a => a.Name == playerDto.Username);
                    existingNewUsernameAlias.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    Context.PlayerAlias.Add(new PlayerAlias
                    {
                        PlayerAliasId = Guid.NewGuid(),
                        Name = playerDto.Username,
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow,
                        PlayerPlayer = player
                    });
                }

                player.Username = playerDto.Username;
            }
            else
            {
                var existingUsernameAlias = player.PlayerAlias.FirstOrDefault(a => a.Name == player.Username);
                if (existingUsernameAlias != null)
                    existingUsernameAlias.LastUsed = DateTime.UtcNow;
            }

            if (IPAddress.TryParse(player.IpAddress, out var ip2) && player.IpAddress != playerDto.IpAddress)
            {
                if (player.PlayerIpAddresses.Any(ip => ip.Address == player.IpAddress))
                {
                    var existingIpAddressAlias = player.PlayerIpAddresses.First(ip => ip.Address == player.IpAddress);
                    existingIpAddressAlias.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    Context.PlayerIpAddresses.Add(new PlayerIpAddresses
                    {
                        PlayerIpAddressId = Guid.NewGuid(),
                        Address = player.IpAddress,
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow,
                        PlayerPlayer = player
                    });
                }

                if (player.PlayerIpAddresses.Any(ip => ip.Address == playerDto.IpAddress))
                {
                    var existingNewIpAddressAlias = player.PlayerIpAddresses.First(ip => ip.Address == playerDto.IpAddress);
                    existingNewIpAddressAlias.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    Context.PlayerIpAddresses.Add(new PlayerIpAddresses
                    {
                        PlayerIpAddressId = Guid.NewGuid(),
                        Address = player.IpAddress,
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow,
                        PlayerPlayer = player
                    });
                }

                player.IpAddress = playerDto.IpAddress;
            }
            else
            {
                var existingIpAddressAlias = player.PlayerIpAddresses.FirstOrDefault(ip => ip.Address == player.IpAddress);
                if (existingIpAddressAlias != null)
                    player.IpAddress = playerDto.IpAddress;
            }

            player.LastSeen = DateTime.UtcNow;

            await Context.SaveChangesAsync();
        }

        return new OkObjectResult(new PlayerApiDto
        {
            Id = player.PlayerId,
            GameType = player.GameType.ToString(),
            Username = player.Username,
            Guid = player.Guid,
            FirstSeen = player.FirstSeen,
            LastSeen = player.LastSeen,
            IpAddress = player.IpAddress
        });
    }
}