using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class GameServersController : Controller
{
    private readonly ILogger<GameServersController> logger;
    private readonly PortalDbContext context;

    public GameServersController(
        ILogger<GameServersController> logger,
        PortalDbContext context)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [Route("api/game-servers")]
    public async Task<IActionResult> GetGameServer(string? gameTypes, string? serverIds, GameServerFilter? filterOption, int skipEntries, int takeEntries, GameServerOrder? order)
    {
        var query = context.GameServers.AsQueryable();

        if (order == null)
            order = GameServerOrder.BannerServerListPosition;

        if (!string.IsNullOrWhiteSpace(gameTypes))
        {
            var split = gameTypes.Split(",");

            var filterByGameTypes = split.Select(gt => Enum.Parse<GameType>(gt)).ToArray().Select(gt => gt.ToGameTypeInt());
            query = query.Where(gs => filterByGameTypes.Contains(gs.GameType)).AsQueryable();
        }

        if (!string.IsNullOrWhiteSpace(serverIds))
        {
            var split = serverIds.Split(",");

            var filterByMonitorIds = split.Select(id => Guid.Parse(id)).ToArray();
            query = query.Where(gs => filterByMonitorIds.Contains(gs.ServerId)).AsQueryable();
        }

        if (filterOption != null)
        {
            switch (filterOption)
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
        }

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
        if (takeEntries != 0) query = query.Take(takeEntries).AsQueryable();

        var results = await query.ToListAsync();

        var result = results.Select(gameServer => gameServer.ToDto());

        return new OkObjectResult(result);
    }

    [HttpGet]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> GetGameServer(Guid? serverId)
    {
        if (serverId == null)
            return new BadRequestResult();

        var gameServer = await context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null)
            return new NotFoundResult();

        var gameServerDto = gameServer.ToDto();

        return new OkObjectResult(gameServerDto);
    }

    [HttpPost]
    [Route("api/game-servers")]
    public async Task<IActionResult> CreateGameServer()
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        List<CreateGameServerDto>? createGameServerDtos;
        try
        {
            createGameServerDtos = JsonConvert.DeserializeObject<List<CreateGameServerDto>>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (createGameServerDtos == null || !createGameServerDtos.Any())
            return new BadRequestResult();

        var gameServers = new List<GameServer>();

        foreach (var createGameServerDto in createGameServerDtos)
        {
            var gameServer = new GameServer
            {
                Title = createGameServerDto.Title,
                GameType = createGameServerDto.GameType.ToGameTypeInt(),
                Hostname = createGameServerDto.Hostname,
                QueryPort = createGameServerDto.QueryPort,
                FtpHostname = createGameServerDto.FtpHostname,
                FtpPort = createGameServerDto.FtpPort,
                FtpUsername = createGameServerDto.FtpUsername,
                FtpPassword = createGameServerDto.FtpPassword,
                RconPassword = createGameServerDto.RconPassword,
                LiveStatusEnabled = createGameServerDto.LiveStatusEnabled,
                BannerServerListPosition = createGameServerDto.BannerServerListPosition,
                ShowOnBannerServerList = createGameServerDto.ShowOnBannerServerList,
                ShowOnPortalServerList = createGameServerDto.ShowOnPortalServerList,
                ShowChatLog = createGameServerDto.ShowChatLog,
                HtmlBanner = createGameServerDto.HtmlBanner
            };

            gameServers.Add(gameServer);
        }

        await context.GameServers.AddRangeAsync(gameServers);
        await context.SaveChangesAsync();

        var entries = gameServers.Select(gs => new GameServerDtoWrapper(gs)).ToList();

        var response = new CollectionResponseDto<GameServerDtoWrapper>
        {
            TotalRecords = gameServers.Count,
            FilteredRecords = gameServers.Count,
            Entries = entries
        };

        return new OkObjectResult(response);
    }

    [HttpPatch]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> UpdateGameServer(Guid serverId)
    {
        var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

        GameServerDto gameServerDto;
        try
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            gameServerDto = JsonConvert.DeserializeObject<GameServerDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not deserialize request body");
            return new BadRequestResult();
        }

        if (gameServerDto == null) return new BadRequestResult();
        if (gameServerDto.Id != serverId) return new BadRequestResult();

        var gameServer = await context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null) return new NotFoundResult();

        gameServer.Title = gameServerDto.Title;
        gameServer.HtmlBanner = gameServerDto.HtmlBanner;
        gameServer.Hostname = gameServerDto.Hostname;
        gameServer.QueryPort = gameServerDto.QueryPort;
        gameServer.FtpHostname = gameServerDto.FtpHostname;
        gameServer.FtpPort = gameServerDto.FtpPort;
        gameServer.FtpUsername = gameServerDto.FtpUsername;
        gameServer.FtpPassword = gameServerDto.FtpPassword;
        gameServer.LiveStatusEnabled = gameServerDto.LiveStatusEnabled;
        gameServer.LiveTitle = gameServerDto.LiveTitle;
        gameServer.LiveMap = gameServerDto.LiveMap;
        gameServer.LiveMod = gameServerDto.LiveMod;
        gameServer.LiveMaxPlayers = gameServerDto.LiveMaxPlayers;
        gameServer.LiveCurrentPlayers = gameServerDto.LiveCurrentPlayers;
        gameServer.LiveLastUpdated = gameServerDto.LiveLastUpdated;
        gameServer.ShowOnBannerServerList = gameServerDto.ShowOnBannerServerList;
        gameServer.BannerServerListPosition = gameServerDto.BannerServerListPosition;
        gameServer.ShowOnPortalServerList = gameServerDto.ShowOnPortalServerList;
        gameServer.ShowChatLog = gameServerDto.ShowChatLog;
        gameServer.RconPassword = gameServerDto.RconPassword;

        await context.SaveChangesAsync();

        return new OkObjectResult(gameServer.ToDto());
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
}