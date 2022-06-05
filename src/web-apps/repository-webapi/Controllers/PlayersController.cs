using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System.Net;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Interfaces;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class PlayersController : ControllerBase, IPlayersApi
{
    private readonly ILogger<PlayersController> logger;
    private readonly PortalDbContext context;
    private readonly IMapper mapper;

    public PlayersController(
        ILogger<PlayersController> logger,
        PortalDbContext context,
            IMapper mapper)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> GetPlayer(Guid playerId)
    {
        var response = await ((IPlayersApi)this).GetPlayer(playerId);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<PlayerDto>> IPlayersApi.GetPlayer(Guid playerId)
    {
        var player = await context.Players
            .Include(p => p.PlayerAliases)
            .Include(p => p.PlayerIpAddresses)
            .Include(p => p.AdminActions)
            .SingleOrDefaultAsync(p => p.PlayerId == playerId);

        if (player == null)
            return new ApiResponseDto<PlayerDto>(HttpStatusCode.NotFound);

        var playerIpAddresses = await context.PlayerIpAddresses
            .Include(ip => ip.PlayerPlayer)
            .Where(ip => ip.Address == player.IpAddress && ip.PlayerPlayerId != player.PlayerId)
            .ToListAsync();

        var result = mapper.Map<PlayerDto>(player);
        result.RelatedPlayerDtos = playerIpAddresses.Select(pip => mapper.Map<RelatedPlayerDto>(pip)).ToList();

        return new ApiResponseDto<PlayerDto>(HttpStatusCode.OK, result);
    }

    [HttpGet]
    [Route("api/players/by-game-type/{gameType}/{guid}")]
    public async Task<IActionResult> GetPlayerByGameType(GameType gameType, string guid)
    {
        var response = await ((IPlayersApi)this).GetPlayerByGameType(gameType, guid);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<PlayerDto>> IPlayersApi.GetPlayerByGameType(GameType gameType, string guid)
    {
        var player = await context.Players
            .Include(p => p.PlayerAliases)
            .Include(p => p.PlayerIpAddresses)
            .Include(p => p.AdminActions)
            .SingleOrDefaultAsync(p => p.GameType == gameType.ToGameTypeInt() && p.Guid == guid);

        if (player == null)
            return new ApiResponseDto<PlayerDto>(HttpStatusCode.NotFound);

        var playerIpAddresses = await context.PlayerIpAddresses
            .Include(ip => ip.PlayerPlayer)
            .Where(ip => ip.Address == player.IpAddress && ip.PlayerPlayerId != player.PlayerId)
            .ToListAsync();

        var result = mapper.Map<PlayerDto>(player);
        result.RelatedPlayerDtos = playerIpAddresses.Select(pip => mapper.Map<RelatedPlayerDto>(pip)).ToList();

        return new ApiResponseDto<PlayerDto>(HttpStatusCode.OK, result);
    }

    [HttpPost]
    [Route("api/players")]
    public async Task<IActionResult> CreatePlayers()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<CreatePlayerDto>? createPlayerDtos;
        try
        {
            createPlayerDtos = JsonConvert.DeserializeObject<List<CreatePlayerDto>>(requestBody);
        }
        catch
        {
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
        }

        if (createPlayerDtos == null || !createPlayerDtos.Any())
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

        var response = await ((IPlayersApi)this).CreatePlayers(createPlayerDtos);

        return response.ToHttpResult();
    }

    Task<ApiResponseDto> IPlayersApi.CreatePlayer(CreatePlayerDto createPlayerDto)
    {
        throw new NotImplementedException();
    }

    async Task<ApiResponseDto> IPlayersApi.CreatePlayers(List<CreatePlayerDto> createPlayerDtos)
    {
        foreach (var createPlayerDto in createPlayerDtos)
        {
            if (await context.Players.AnyAsync(p => p.GameType == createPlayerDto.GameType.ToGameTypeInt() && p.Guid == createPlayerDto.Guid))
                return new ApiResponseDto(HttpStatusCode.Conflict, $"Player with gameType '{createPlayerDto.GameType}' and guid '{createPlayerDto.Guid}' already exists");

            var player = mapper.Map<Player>(createPlayerDto);

            if (IPAddress.TryParse(createPlayerDto.IpAddress, out var ip))
            {
                player.IpAddress = ip.ToString();

                player.PlayerIpAddresses = new List<PlayerIpAddress>
                {
                    new PlayerIpAddress
                    {
                        Address = ip.ToString(),
                        Added = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow
                    }
                };
            }

            player.PlayerAliases = new List<PlayerAlias>
            {
                new PlayerAlias
                {
                    Name = createPlayerDto.Username.Trim(),
                    Added = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow
                }
            };

            await context.Players.AddAsync(player);
        }

        await context.SaveChangesAsync();

        return new ApiResponseDto(HttpStatusCode.OK);
    }

    [HttpPatch]
    [Route("api/players/{playerId}")]
    public async Task<IActionResult> UpdatePlayer(Guid playerId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        EditPlayerDto? editPlayerDto;
        try
        {
            editPlayerDto = JsonConvert.DeserializeObject<EditPlayerDto>(requestBody);
        }
        catch
        {
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
        }

        if (editPlayerDto == null)
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

        if (editPlayerDto.Id != playerId)
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request entity identifiers did not match").ToHttpResult();

        var response = await ((IPlayersApi)this).UpdatePlayer(editPlayerDto);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto> IPlayersApi.UpdatePlayer(EditPlayerDto editPlayerDto)
    {
        var player = await context.Players
                .Include(p => p.PlayerAliases)
                .Include(p => p.PlayerIpAddresses)
                .SingleOrDefaultAsync(p => p.PlayerId == editPlayerDto.Id);

        if (player == null)
            return new ApiResponseDto<ReportDto>(HttpStatusCode.NotFound);

        player.Username = editPlayerDto.Username;
        player.IpAddress = editPlayerDto.IpAddress ?? null;
        player.LastSeen = DateTime.UtcNow;

        var playerAlias = player.PlayerAliases.FirstOrDefault(a => a.Name == editPlayerDto.Username);
        if (playerAlias != null)
        {
            playerAlias.LastUsed = DateTime.UtcNow;
        }
        else
        {
            player.PlayerAliases.Add(new PlayerAlias
            {
                Name = editPlayerDto.Username,
                Added = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            });
        }

        var playerIpAddress = player.PlayerIpAddresses.FirstOrDefault(a => a.Address == editPlayerDto.IpAddress);
        if (playerIpAddress != null)
        {
            playerIpAddress.LastUsed = DateTime.UtcNow;
        }
        else
        {
            player.PlayerIpAddresses.Add(new PlayerIpAddress
            {
                Address = editPlayerDto.IpAddress,
                Added = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        return new ApiResponseDto(HttpStatusCode.OK);
    }

    [HttpGet]
    [Route("api/players/search")]
    public async Task<IActionResult> SearchPlayers(string? gameType, string? filter, string? filterString, int takeEntries, int skipEntries, string? order)
    {
        if (!Enum.TryParse(gameType, out GameType legacyGameType))
        {
            legacyGameType = GameType.Unknown;
        }

        if (string.IsNullOrWhiteSpace(order))
            order = "LastSeenDesc";

        if (filter == null)
            filter = string.Empty;

        if (filterString == null)
            filterString = string.Empty;

        var query = context.Players.AsQueryable();
        query = ApplySearchFilter(query, legacyGameType, string.Empty, string.Empty);
        var totalCount = await query.CountAsync();

        query = ApplySearchFilter(query, legacyGameType, filter, filterString);
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

    private IQueryable<Player> ApplySearchFilter(IQueryable<Player> players, GameType gameType, string filter, string filterString)
    {
        players = players.AsQueryable();

        if (gameType != GameType.Unknown) players = players.Where(p => p.GameType == gameType.ToGameTypeInt()).AsQueryable();

        if (filter != "None" && !string.IsNullOrWhiteSpace(filterString))
            switch (filter)
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
        else if (filter == "IpAddress") players = players.Where(p => p.IpAddress != "" && p.IpAddress != null).AsQueryable();

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
            .OrderByDescending(aa => aa.Created)
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

        var adminAction = await context.AdminActions
            .Include(aa => aa.UserProfile)
            .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

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

    Task<PlayersSearchResponseDto?> IPlayersApi.SearchPlayers(string gameType, string filter, string filterString, int takeEntries, int skipEntries, string? order)
    {
        throw new NotImplementedException();
    }

    Task<List<AdminActionDto>?> IPlayersApi.GetAdminActionsForPlayer(Guid playerId)
    {
        throw new NotImplementedException();
    }

    Task<AdminActionDto?> IPlayersApi.CreateAdminActionForPlayer(AdminActionDto adminAction)
    {
        throw new NotImplementedException();
    }

    Task<AdminActionDto?> IPlayersApi.UpdateAdminActionForPlayer(AdminActionDto adminAction)
    {
        throw new NotImplementedException();
    }

}