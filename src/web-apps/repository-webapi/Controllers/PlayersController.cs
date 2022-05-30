using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class PlayersController : ControllerBase
{
    private readonly ILogger<PlayersController> logger;
    private readonly PortalDbContext context;

    public PlayersController(
        ILogger<PlayersController> logger,
        PortalDbContext context)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> GetPlayer(Guid playerId)
    {
        var player = await context.Players.SingleOrDefaultAsync(p => p.PlayerId == playerId);

        if (player == null) return new NotFoundResult();

        var playerDto = new PlayerDto
        {
            Id = player.PlayerId,
            GameType = player.GameType.ToGameType(),
            Username = player.Username,
            Guid = player.Guid,
            FirstSeen = player.FirstSeen,
            LastSeen = player.LastSeen,
            IpAddress = player.IpAddress
        };

        return new OkObjectResult(playerDto);
    }

    [HttpGet]
    [Route("api/players/{playerId}/aliases")]
    public async Task<IActionResult> GetPlayerAliases(Guid playerId)
    {
        var player = await context.Players
            .Include(p => p.PlayerAliases)
            .SingleAsync(p => p.PlayerId == playerId);

        var result = player.PlayerAliases.Select(alias => new AliasDto
        {
            Name = alias.Name,
            Added = alias.Added,
            LastUsed = alias.LastUsed
        }).ToList();

        return new OkObjectResult(result);
    }

    [HttpGet]
    [Route("api/players/{playerId}/ip-addresses")]
    public async Task<IActionResult> GetPlayerIpAddresses(Guid playerId)
    {
        var player = await context.Players
            .Include(p => p.PlayerIpAddresses)
            .SingleAsync(p => p.PlayerId == playerId);

        var result = player.PlayerIpAddresses.Select(address => new IpAddressDto
        {
            Address = address.Address,
            Added = address.Added,
            LastUsed = address.LastUsed
        }).ToList();

        return new OkObjectResult(result);
    }

    [HttpGet]
    [Route("api/players/{playerId}/related-players")]
    public async Task<IActionResult> GetRelatedPlayers(Guid playerId, string ipAddress)
    {
        var playerIpAddresses = await context.PlayerIpAddresses.Include(ip => ip.PlayerPlayer)
            .Where(ip => ip.Address == ipAddress && ip.PlayerPlayerId != playerId)
            .ToListAsync();

        var result = playerIpAddresses.Select(pip => new RelatedPlayerDto
        {
            GameType = pip.PlayerPlayer.GameType.ToGameType(),
            Username = pip.PlayerPlayer.Username,
            PlayerId = pip.PlayerPlayer.PlayerId,
            IpAddress = pip.Address
        }).ToList();

        return new OkObjectResult(result);
    }

    [HttpGet]
    [Route("api/players/by-game-type/{gameType}/{playerGuid}")]
    public async Task<IActionResult> GetPlayerByGameType(GameType gameType, string playerGuid)
    {
        var player = await context.Players.SingleOrDefaultAsync(p => p.GameType == gameType.ToGameTypeInt() && p.Guid == playerGuid);

        if (player == null) return new NotFoundResult();

        var playerDto = new PlayerDto
        {
            Id = player.PlayerId,
            GameType = player.GameType.ToGameType(),
            Username = player.Username,
            Guid = player.Guid,
            FirstSeen = player.FirstSeen,
            LastSeen = player.LastSeen,
            IpAddress = player.IpAddress
        };

        return new OkObjectResult(playerDto);
    }

    [HttpPost]
    [Route("api/players")]
    public async Task<IActionResult> CreatePlayer()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<PlayerDto> playerDtos;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            playerDtos = JsonConvert.DeserializeObject<List<PlayerDto>>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (playerDtos == null) return new BadRequestResult();

        foreach (var player in playerDtos)
        {
            var existingPlayer = await context.Players.SingleOrDefaultAsync(p => p.GameType == player.GameType.ToGameTypeInt() && p.Guid == player.Guid);

            if (existingPlayer != null) return new ConflictObjectResult(existingPlayer);

            var player2 = new Player
            {
                Username = player.Username.Trim(),
                Guid = player.Guid.ToLower().Trim(),
                GameType = player.GameType.ToGameTypeInt(),
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };

            if (IPAddress.TryParse(player.IpAddress, out var ip))
            {
                player2.IpAddress = ip.ToString();

                player2.PlayerIpAddresses = new List<PlayerIpAddress>
                {
                    new PlayerIpAddress
                    {
                        PlayerIpAddressId = Guid.NewGuid(),
                        Address = ip.ToString(),
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow
                    }
                };
            }

            player2.PlayerAliases = new List<PlayerAlias>
            {
                new PlayerAlias
                {
                    PlayerAliasId = Guid.NewGuid(),
                    Name = player.Username.Trim(),
                    Added = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow
                }
            };

            await context.Players.AddAsync(player2);
        }

        await context.SaveChangesAsync();

        return new OkObjectResult(playerDtos);
    }

    [HttpPatch]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> UpdatePlayer(Guid playerId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        PlayerDto playerDto;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            playerDto = JsonConvert.DeserializeObject<PlayerDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (playerDto == null) return new BadRequestResult();
        if (playerDto.Id != playerId) return new BadRequestResult();

        playerDto.Username = playerDto.Username.Trim();

        var player = await context.Players
                .Include(p => p.PlayerAliases)
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
                if (player.PlayerAliases.Any(a => a.Name == player.Username))
                {
                    var existingUsernameAlias = player.PlayerAliases.First(a => a.Name == player.Username);
                    existingUsernameAlias.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    context.PlayerAliases.Add(new PlayerAlias
                    {
                        PlayerAliasId = Guid.NewGuid(),
                        Name = player.Username,
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow,
                        PlayerPlayer = player
                    });
                }

                if (player.PlayerAliases.Any(a => a.Name == playerDto.Username))
                {
                    var existingNewUsernameAlias = player.PlayerAliases.First(a => a.Name == playerDto.Username);
                    existingNewUsernameAlias.LastUsed = DateTime.UtcNow;
                }
                else
                {
                    context.PlayerAliases.Add(new PlayerAlias
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
                var existingUsernameAlias = player.PlayerAliases.FirstOrDefault(a => a.Name == player.Username);
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
                    context.PlayerIpAddresses.Add(new PlayerIpAddress
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
                    context.PlayerIpAddresses.Add(new PlayerIpAddress
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

            await context.SaveChangesAsync();
        }

        return new OkObjectResult(new PlayerDto
        {
            Id = player.PlayerId,
            GameType = player.GameType.ToGameType(),
            Username = player.Username,
            Guid = player.Guid,
            FirstSeen = player.FirstSeen,
            LastSeen = player.LastSeen,
            IpAddress = player.IpAddress
        });
    }

    [HttpGet]
    [Route("api/players/search")]
    public async Task<IActionResult> SearchPlayers(string? gameType, string? filterType, string? filterString, int takeEntries, int skipEntries, string? order)
    {
        if (!Enum.TryParse(gameType, out GameType legacyGameType))
        {
            legacyGameType = GameType.Unknown;
        }

        if (string.IsNullOrWhiteSpace(order))
            order = "LastSeenDesc";

        if (filterType == null)
            filterType = string.Empty;

        if (filterString == null)
            filterString = string.Empty;

        var query = context.Players.AsQueryable();
        query = ApplySearchFilter(query, legacyGameType, string.Empty, string.Empty);
        var totalCount = await query.CountAsync();

        query = ApplySearchFilter(query, legacyGameType, filterType, filterString);
        var filteredCount = await query.CountAsync();

        query = ApplySearchOrderAndLimits(query, order, skipEntries, takeEntries);
        var searchResults = await query.ToListAsync();

        var entries = searchResults.Select(p => new PlayerDto()
        {
            Id = p.PlayerId,
            GameType = p.GameType.ToGameType(),
            Username = p.Username,
            Guid = p.Guid,
            FirstSeen = p.FirstSeen,
            LastSeen = p.LastSeen,
            IpAddress = p.IpAddress
        }).ToList();

        var response = new PlayersSearchResponseDto
        {
            TotalRecords = totalCount,
            FilteredRecords = filteredCount,
            Entries = entries
        };

        return new OkObjectResult(response);
    }

    private IQueryable<Player> ApplySearchFilter(IQueryable<Player> players, GameType gameType, string filterType, string filterString)
    {
        players = players.AsQueryable();

        if (gameType != GameType.Unknown) players = players.Where(p => p.GameType == gameType.ToGameTypeInt()).AsQueryable();

        if (filterType != "None" && !string.IsNullOrWhiteSpace(filterString))
            switch (filterType)
            {
                case "UsernameAndGuid":
                    players = players.Where(p => p.Username.Contains(filterString) ||
                                                 p.Guid.Contains(filterString) ||
                                                 p.PlayerAliases.Any(a => a.Name.Contains(filterString)))
                        .AsQueryable();
                    break;
                case "IpAddress":
                    players = players.Where(p => p.IpAddress.Contains(filterString) ||
                                                 p.PlayerIpAddresses.Any(ip => ip.Address.Contains(filterString)))
                        .AsQueryable();
                    break;
            }
        else if (filterType == "IpAddress") players = players.Where(p => p.IpAddress != "" && p.IpAddress != null).AsQueryable();

        return players;
    }

    private IQueryable<Player> ApplySearchOrderAndLimits(IQueryable<Player> players, string order, int skipEntries, int takeEntries)
    {
        switch (order)
        {
            case "UsernameAsc":
                players = players.OrderBy(p => p.Username).AsQueryable();
                break;
            case "UsernameDesc":
                players = players.OrderByDescending(p => p.Username).AsQueryable();
                break;
            case "FirstSeenAsc":
                players = players.OrderBy(p => p.FirstSeen).AsQueryable();
                break;
            case "FirstSeenDesc":
                players = players.OrderByDescending(p => p.FirstSeen).AsQueryable();
                break;
            case "LastSeenAsc":
                players = players.OrderBy(p => p.LastSeen).AsQueryable();
                break;
            case "LastSeenDesc":
                players = players.OrderByDescending(p => p.LastSeen).AsQueryable();
                break;
            case "GameTypeAsc":
                players = players.OrderBy(p => p.GameType).AsQueryable();
                break;
            case "GameTypeDesc":
                players = players.OrderByDescending(p => p.GameType).AsQueryable();
                break;
        }

        players = players.Skip(skipEntries).AsQueryable();

        if (takeEntries != 0) players = players.Take(takeEntries).AsQueryable();

        return players;
    }


    [HttpGet]
    [Route("api/players/{playerId}/admin-actions")]
    public async Task<IActionResult> GetAdminActionsForPlayer(Guid playerId)
    {
        var results = await context.AdminActions
            .Include(aa => aa.PlayerPlayer)
            .Include(aa => aa.UserProfile)
            .Where(aa => aa.PlayerPlayerId == playerId)
            .ToListAsync();

        var result = results.Select(adminAction => adminAction.ToDto());

        return new OkObjectResult(result);
    }

    [HttpPost]
    [Route("api/players/{playerId}/admin-actions")]
    public async Task<IActionResult> CreateAdminActionForPlayer(Guid playerId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        AdminActionDto adminActionDto;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            adminActionDto = JsonConvert.DeserializeObject<AdminActionDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (adminActionDto == null) return new BadRequestResult();
        if (adminActionDto.PlayerId != playerId) return new BadRequestResult();

        var player = await context.Players.SingleOrDefaultAsync(p => p.PlayerId == adminActionDto.PlayerId);

        UserProfile admin = null;
        if (!string.IsNullOrWhiteSpace(adminActionDto.AdminId))
        {
            admin = await context.UserProfiles.SingleOrDefaultAsync(u => u.XtremeIdiotsForumId == adminActionDto.AdminId);
        }

        var adminAction = new AdminAction
        {
            PlayerPlayer = player,
            UserProfile = admin,
            Type = adminActionDto.Type.ToAdminActionTypeInt(),
            Text = adminActionDto.Text,
            Created = DateTime.UtcNow,
            Expires = adminActionDto.Expires,
            ForumTopicId = adminActionDto.ForumTopicId
        };

        context.AdminActions.Add(adminAction);
        await context.SaveChangesAsync();

        return new OkObjectResult(adminActionDto);
    }

    [HttpPatch]
    [Route("api/players/{playerId}/admin-actions/{adminActionId}")]
    public async Task<IActionResult> UpdateAdminActionForPlayer(Guid playerId, Guid adminActionId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        AdminActionDto adminActionDto;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            adminActionDto = JsonConvert.DeserializeObject<AdminActionDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (adminActionDto == null) return new BadRequestResult();
        if (adminActionDto.PlayerId != playerId) return new BadRequestResult();

        var adminAction = await context.AdminActions.SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

        if (adminAction == null)
            throw new NullReferenceException(nameof(adminAction));

        adminAction.Text = adminActionDto.Text;
        adminAction.Expires = adminActionDto.Expires;

        if (adminAction.UserProfile.XtremeIdiotsForumId != adminActionDto.AdminId)
        {
            if (string.IsNullOrWhiteSpace(adminActionDto.AdminId))
                adminAction.UserProfile = null;
            else
            {
                var admin = await context.UserProfiles.SingleOrDefaultAsync(u => u.XtremeIdiotsForumId == adminActionDto.AdminId);

                if (admin == null)
                    throw new NullReferenceException(nameof(admin));

                adminAction.UserProfile = admin;
            }
        }

        if (adminActionDto.ForumTopicId != 0)
            adminAction.ForumTopicId = adminActionDto.ForumTopicId;

        await context.SaveChangesAsync();

        return new OkObjectResult(adminActionDto);
    }
}