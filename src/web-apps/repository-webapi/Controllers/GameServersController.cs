using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers;

[ApiController]
[Authorize(Roles = "ServiceAccount")]
public class GameServersController : Controller
{
    public GameServersController(PortalDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public PortalDbContext Context { get; }

    [HttpGet]
    [Route("api/game-servers")]
    public async Task<IActionResult> GetGameServer(string? gameTypes, string? serverIds, GameServerFilter? filterOption, int skipEntries, int takeEntries, GameServerOrder? order)
    {
        var query = Context.GameServers.AsQueryable();

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

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

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
            var existingGameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == gameServer.Id);
            if (existingGameServer != null) return new ConflictObjectResult(existingGameServer);

            if (string.IsNullOrWhiteSpace(gameServer.Title)) gameServer.Title = "to-be-updated";

            if (string.IsNullOrWhiteSpace(gameServer.Hostname)) gameServer.Hostname = "127.0.0.1";

            //await Context.GameServers.AddAsync(gameServer);
        }

        //await Context.SaveChangesAsync();

        return new OkObjectResult(gameServers);
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
            return new BadRequestObjectResult(ex);
        }

        if (gameServerDto == null) return new BadRequestResult();
        if (gameServerDto.Id != serverId) return new BadRequestResult();

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null) return new NotFoundResult();

        gameServer.Title = gameServerDto.Title;
        gameServer.HtmlBanner = gameServerDto.HtmlBanner;
        gameServer.Hostname = gameServerDto.Hostname;
        gameServer.QueryPort = gameServerDto.QueryPort;
        gameServer.FtpHostname = gameServerDto.FtpHostname;
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

        await Context.SaveChangesAsync();

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
            return new BadRequestObjectResult(ex);
        }

        if (banFileMonitorDto == null) return new BadRequestResult();

        var server = await Context.GameServers.SingleOrDefaultAsync(s => s.ServerId == serverId);

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

        Context.BanFileMonitors.Add(banFileMonitor);
        await Context.SaveChangesAsync();

        var result = banFileMonitor.ToDto();

        return new OkObjectResult(result);
    }

    [HttpDelete]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> DeleteGameServer(Guid? serverId)
    {
        if (serverId == null)
            return new BadRequestResult();

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null)
            return new NotFoundResult();

        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.BanFileMonitors)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.ChatLogs)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.FileMonitors)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.GameServerEvents)}] WHERE [GameServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.GameServerMaps)}] WHERE [GameServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.GameServerStats)}] WHERE [GameServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.LivePlayers)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");
        await Context.Database.ExecuteSqlRawAsync($"DELETE FROM [dbo].[{nameof(Context.RconMonitors)}] WHERE [GameServer_ServerId] = '{gameServer.ServerId}'");

        Context.GameServers.Remove(gameServer);

        await Context.SaveChangesAsync();

        return new OkResult();
    }
}