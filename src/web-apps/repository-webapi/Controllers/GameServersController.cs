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
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Reports;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class GameServersController : Controller, IGameServersApi
{
    private readonly ILogger<GameServersController> logger;
    private readonly PortalDbContext context;
    private readonly IMapper mapper;

    public GameServersController(
        ILogger<GameServersController> logger,
        PortalDbContext context,
            IMapper mapper)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> GetGameServer(Guid serverId)
    {
        var response = await ((IGameServersApi)this).GetGameServer(serverId);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<GameServerDto>> IGameServersApi.GetGameServer(Guid serverId)
    {
        var gameServer = await context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null)
            return new ApiResponseDto<GameServerDto>(HttpStatusCode.NotFound);

        var result = mapper.Map<GameServerDto>(gameServer);

        return new ApiResponseDto<GameServerDto>(HttpStatusCode.OK, result);
    }

    [HttpGet]
    [Route("api/game-servers")]
    public async Task<IActionResult> GetGameServer(string? gameTypes, string? serverIds, GameServerFilter? filter, int? skipEntries, int? takeEntries, GameServerOrder? order)
    {
        if (!skipEntries.HasValue)
            skipEntries = 0;

        if (!takeEntries.HasValue)
            takeEntries = 20;

        GameType[]? gameTypesFilter = null;
        if (!string.IsNullOrWhiteSpace(gameTypes))
        {
            var split = gameTypes.Split(",");
            gameTypesFilter = split.Select(gt => Enum.Parse<GameType>(gt)).ToArray();
        }

        Guid[]? serverIdsFilter = null;
        if (!string.IsNullOrWhiteSpace(serverIds))
        {
            var split = serverIds.Split(",");
            serverIdsFilter = split.Select(id => Guid.Parse(id)).ToArray();
        }

        var response = await ((IGameServersApi)this).GetGameServers(gameTypesFilter, serverIdsFilter, filter, skipEntries.Value, takeEntries.Value, order);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto<GameServersCollectionDto>> IGameServersApi.GetGameServers(GameType[]? gameTypes, Guid[]? serverIds, GameServerFilter? filter, int skipEntries, int takeEntries, GameServerOrder? order)
    {
        var query = context.GameServers.AsQueryable();
        query = ApplyFilter(query, gameTypes, null, null);
        var totalCount = await query.CountAsync();

        query = ApplyFilter(query, gameTypes, serverIds, filter);
        var filteredCount = await query.CountAsync();

        query = ApplyOrderAndLimits(query, skipEntries, takeEntries, order);
        var results = await query.ToListAsync();

        var entries = results.Select(m => mapper.Map<GameServerDto>(m)).ToList();

        var result = new GameServersCollectionDto
        {
            TotalRecords = totalCount,
            FilteredRecords = filteredCount,
            Entries = entries
        };

        return new ApiResponseDto<GameServersCollectionDto>(HttpStatusCode.OK, result);
    }

    Task<ApiResponseDto> IGameServersApi.CreateGameServer(CreateGameServerDto createGameServerDto)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("api/game-servers")]
    public async Task<IActionResult> CreateGameServers()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<CreateGameServerDto>? createGameServerDtos;
        try
        {
            createGameServerDtos = JsonConvert.DeserializeObject<List<CreateGameServerDto>>(requestBody);
        }
        catch
        {
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
        }

        if (createGameServerDtos == null || !createGameServerDtos.Any())
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null or did not contain any entries").ToHttpResult();

        var response = await ((IGameServersApi)this).CreateGameServers(createGameServerDtos);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto> IGameServersApi.CreateGameServers(List<CreateGameServerDto> createGameServerDtos)
    {
        var gameServers = createGameServerDtos.Select(gs => mapper.Map<GameServer>(gs)).ToList();

        await context.GameServers.AddRangeAsync(gameServers);
        await context.SaveChangesAsync();

        return new ApiResponseDto(HttpStatusCode.OK);
    }

    [HttpPatch]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> UpdateGameServer(Guid serverId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        EditGameServerDto? editGameServerDto;
        try
        {
            editGameServerDto = JsonConvert.DeserializeObject<EditGameServerDto>(requestBody);
        }
        catch
        {
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Could not deserialize request body").ToHttpResult();
        }

        if (editGameServerDto == null)
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request body was null").ToHttpResult();

        if (editGameServerDto.Id != serverId)
            return new ApiResponseDto(HttpStatusCode.BadRequest, "Request entity identifiers did not match").ToHttpResult();

        var response = await ((IGameServersApi)this).UpdateGameServer(editGameServerDto);

        return response.ToHttpResult();
    }

    async Task<ApiResponseDto> IGameServersApi.UpdateGameServer(EditGameServerDto editGameServerDto)
    {
        var gameServer = await context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == editGameServerDto.Id);

        if (gameServer == null)
            return new ApiResponseDto<ReportDto>(HttpStatusCode.NotFound);

        mapper.Map(editGameServerDto, gameServer);

        await context.SaveChangesAsync();

        return new ApiResponseDto(HttpStatusCode.OK);
    }









    [HttpPost]
    [Route("api/game-servers/{serverId}/ban-file-monitors")]
    public async Task<IActionResult> CreateBanFileMonitorForGameServer(Guid serverId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        BanFileMonitorDto banFileMonitorDto;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            banFileMonitorDto = JsonConvert.DeserializeObject<BanFileMonitorDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (banFileMonitorDto == null) return new BadRequestResult();

        var server = await context.GameServers.SingleOrDefaultAsync(s => s.ServerId == serverId);

        if (server == null)
            throw new NullReferenceException(nameof(server));

        var banFileMonitor = new BanFileMonitor
        {
            BanFileMonitorId = Guid.NewGuid(),
            FilePath = banFileMonitorDto.FilePath,
            //RemoteFileSize = banFileMonitorDto.RemoteFileSize,
            LastSync = DateTime.UtcNow.AddHours(-4),
            //LastError = string.Empty,
            GameServerServer = server
        };

        context.BanFileMonitors.Add(banFileMonitor);
        await context.SaveChangesAsync();

        var result = banFileMonitor.ToDto();

        return new OkObjectResult(result);
    }

    [HttpDelete]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> DeleteGameServer(Guid? serverId)
    {
        if (serverId == null)
            return new BadRequestResult();

        var gameServer = await context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null)
            return new NotFoundResult();

        await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.BanFileMonitors)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.ChatLogs)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.GameServerEvents)}] WHERE [GameServerId] = '{gameServer.ServerId}'");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.GameServerStats)}] WHERE [GameServerId] = '{gameServer.ServerId}'");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(context.LivePlayers)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");

        context.GameServers.Remove(gameServer);

        await context.SaveChangesAsync();

        return new OkResult();
    }

    public class GameServerDtoWrapper : GameServerDto
    {
        public GameServerDtoWrapper(GameServer gameServer)
        {
            Id = gameServer.ServerId;
            Title = gameServer.Title;
            HtmlBanner = gameServer.HtmlBanner;
            GameType = gameServer.GameType.ToGameType();
            Hostname = gameServer.Hostname;
            QueryPort = gameServer.QueryPort;
            FtpHostname = gameServer.FtpHostname;
            FtpPort = gameServer.FtpPort;
            FtpUsername = gameServer.FtpUsername;
            FtpPassword = gameServer.FtpPassword;
            LiveStatusEnabled = gameServer.LiveStatusEnabled;
            LiveTitle = gameServer.LiveTitle;
            LiveMap = gameServer.LiveMap;
            LiveMod = gameServer.LiveMod;
            LiveMaxPlayers = gameServer.LiveMaxPlayers;
            LiveCurrentPlayers = gameServer.LiveCurrentPlayers;
            LiveLastUpdated = gameServer.LiveLastUpdated;
            ShowOnBannerServerList = gameServer.ShowOnBannerServerList;
            BannerServerListPosition = gameServer.BannerServerListPosition;
            ShowOnPortalServerList = gameServer.ShowOnPortalServerList;
            ShowChatLog = gameServer.ShowChatLog;
            RconPassword = gameServer.RconPassword;
        }
    }

    private IQueryable<GameServer> ApplyFilter(IQueryable<GameServer> query, GameType[]? gameTypes, Guid[]? serverIds, GameServerFilter? filter)
    {
        if (gameTypes != null && gameTypes.Length > 0)
        {
            var gameTypeInts = gameTypes.Select(gt => gt.ToGameTypeInt()).ToArray();
            query = query.Where(gs => gameTypeInts.Contains(gs.GameType)).AsQueryable();
        }

        if (serverIds != null && serverIds.Length > 0)
            query = query.Where(gs => serverIds.Contains(gs.ServerId)).AsQueryable();

        switch (filter)
        {
            case GameServerFilter.ShowOnPortalServerList:
                query = query.Where(s => s.ShowOnPortalServerList).AsQueryable();
                break;
            case GameServerFilter.ShowOnBannerServerList:
                query = query.Where(s => s.ShowOnBannerServerList).AsQueryable();
                break;
            case GameServerFilter.LiveStatusEnabled:
                query = query.Where(s => s.LiveStatusEnabled).AsQueryable();
                break;
        }

        return query;
    }

    private IQueryable<GameServer> ApplyOrderAndLimits(IQueryable<GameServer> query, int skipEntries, int takeEntries, GameServerOrder? order)
    {
        switch (order)
        {
            case GameServerOrder.BannerServerListPosition:
                query = query.OrderBy(gs => gs.BannerServerListPosition).AsQueryable();
                break;
            case GameServerOrder.GameType:
                query = query.OrderBy(gs => gs.GameType).AsQueryable();
                break;
        }

        query = query.Skip(skipEntries).AsQueryable();
        query = query.Take(takeEntries).AsQueryable();

        return query;
    }

    Task<BanFileMonitorDto?> IGameServersApi.CreateBanFileMonitorForGameServer(Guid serverId, BanFileMonitorDto banFileMonitor)
    {
        throw new NotImplementedException();
    }

    Task IGameServersApi.DeleteGameServer(Guid id)
    {
        throw new NotImplementedException();
    }


}