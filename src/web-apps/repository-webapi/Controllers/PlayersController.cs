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
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class PlayersController : ControllerBase, IPlayersApi
{
    private readonly PortalDbContext context;
    private readonly IMapper mapper;

    public PlayersController(
        PortalDbContext context,
        IMapper mapper)
    {
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

    [HttpGet]
    [Route("api/players")]
    public async Task<IActionResult> GetPlayers(GameType? gameType, PlayersFilter? filter, string? filterString, int? skipEntries, int? takeEntries, PlayersOrder? order)
    {
        if (!skipEntries.HasValue)
            skipEntries = 0;

        if (!takeEntries.HasValue)
            takeEntries = 20;

        var response = await ((IPlayersApi)this).GetPlayers(gameType, filter, filterString, skipEntries.Value, takeEntries.Value, order);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<PlayersCollectionDto>> IPlayersApi.GetPlayers(GameType? gameType, PlayersFilter? filter, string? filterString, int skipEntries, int takeEntries, PlayersOrder? order)
    {
        var query = context.Players
            .Include(p => p.PlayerAliases)
            .Include(p => p.PlayerIpAddresses)
            .Include(p => p.AdminActions)
            .AsQueryable();

        query = ApplyFilter(query, gameType, null, null);
        var totalCount = await query.CountAsync();

        query = ApplyFilter(query, gameType, filter, filterString);
        var filteredCount = await query.CountAsync();

        query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
        var results = await query.ToListAsync();

        var entries = results.Select(p => mapper.Map<PlayerDto>(p)).ToList();

        var result = new PlayersCollectionDto
        {
            TotalRecords = totalCount,
            FilteredRecords = filteredCount,
            Entries = entries
        };

        return new ApiResponseDto<PlayersCollectionDto>(HttpStatusCode.OK, result);
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
            return new ApiResponseDto(HttpStatusCode.NotFound);

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

    private IQueryable<Player> ApplyFilter(IQueryable<Player> query, GameType? gameType, PlayersFilter? filter, string? filterString)
    {
        if (gameType.HasValue)
            query = query.Where(p => p.GameType == gameType.Value.ToGameTypeInt()).AsQueryable();

        if (filter.HasValue && !string.IsNullOrWhiteSpace(filterString))
        {
            switch (filter)
            {
                case PlayersFilter.UsernameAndGuid:
                    query = query.Where(p => p.Username.Contains(filterString) || p.Guid.Contains(filterString) || p.PlayerAliases.Any(a => a.Name.Contains(filterString))).AsQueryable();
                    break;
                case PlayersFilter.IpAddress:
                    query = query.Where(p => p.IpAddress.Contains(filterString) || p.PlayerIpAddresses.Any(ip => ip.Address.Contains(filterString))).AsQueryable();
                    break;
            }
        }

        return query;
    }

    private IQueryable<Player> ApplyOrderAndLimits(IQueryable<Player> query, int skipEntries, int takeEntries, PlayersOrder? order)
    {
        switch (order)
        {
            case PlayersOrder.UsernameAsc:
                query = query.OrderBy(p => p.Username).AsQueryable();
                break;
            case PlayersOrder.UsernameDesc:
                query = query.OrderByDescending(p => p.Username).AsQueryable();
                break;
            case PlayersOrder.FirstSeenAsc:
                query = query.OrderBy(p => p.FirstSeen).AsQueryable();
                break;
            case PlayersOrder.FirstSeenDesc:
                query = query.OrderByDescending(p => p.FirstSeen).AsQueryable();
                break;
            case PlayersOrder.LastSeenAsc:
                query = query.OrderBy(p => p.LastSeen).AsQueryable();
                break;
            case PlayersOrder.LastSeenDesc:
                query = query.OrderByDescending(p => p.LastSeen).AsQueryable();
                break;
            case PlayersOrder.GameTypeAsc:
                query = query.OrderBy(p => p.GameType).AsQueryable();
                break;
            case PlayersOrder.GameTypeDesc:
                query = query.OrderByDescending(p => p.GameType).AsQueryable();
                break;
        }

        query = query.Skip(skipEntries).AsQueryable();
        query = query.Take(takeEntries).AsQueryable();

        return query;
    }
}