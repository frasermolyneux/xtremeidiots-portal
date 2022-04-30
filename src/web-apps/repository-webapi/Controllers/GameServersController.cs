using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

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
    public async Task<IActionResult> GetGameServer(string? gameTypes, string? serverIds, string? filterOption, int skipEntries, int takeEntries, string? order)
    {
        var query = Context.GameServers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(gameTypes))
        {
            var split = gameTypes.Split(",");

            var filterByGameTypes = split.Select(gt => Enum.Parse<GameType>(gt)).ToArray();
            query = query.Where(gs => filterByGameTypes.Contains(gs.GameType)).AsQueryable();
        }

        if (!string.IsNullOrWhiteSpace(serverIds))
        {
            var split = serverIds.Split(",");

            var filterByMonitorIds = split.Select(id => Guid.Parse(id)).ToArray();
            query = query.Where(gs => filterByMonitorIds.Contains(gs.ServerId)).AsQueryable();
        }

        if (!string.IsNullOrWhiteSpace(filterOption))
        {
            switch (filterOption)
            {
                case "BannerServerListPosition":
                    query = query.Where(s => s.ShowOnPortalServerList);
                    break;
            }
        }

        switch (order)
        {
            case "BannerServerListPosition":
                query = query.OrderBy(gs => gs.BannerServerListPosition).AsQueryable();
                break;
            case "GameType":
                query = query.OrderBy(gs => gs.GameType).AsQueryable();
                break;
        }

        query = query.Skip(skipEntries).AsQueryable();
        if (takeEntries != 0) query = query.Take(takeEntries).AsQueryable();

        var results = await query.ToListAsync();

        var result = results.Select(gameServer => new GameServerDto
        {
            Id = gameServer.ServerId,
            Title = gameServer.Title,
            GameType = gameServer.GameType.ToString(),
            Hostname = gameServer.Hostname,
            QueryPort = gameServer.QueryPort,
            FtpHostname = gameServer.FtpHostname,
            FtpUsername = gameServer.FtpUsername,
            FtpPassword = gameServer.FtpPassword,
            RconPassword = gameServer.RconPassword,
            ShowOnBannerServerList = gameServer.ShowOnBannerServerList,
            HtmlBanner = gameServer.HtmlBanner,
            BannerServerListPosition = gameServer.BannerServerListPosition,
            ShowOnPortalServerList = gameServer.ShowOnPortalServerList,
            ShowChatLog = gameServer.ShowChatLog
        });

        return new OkObjectResult(result);
    }

    [HttpGet]
    [Route("api/game-servers/{serverId}")]
    public async Task<IActionResult> GetGameServer(Guid? serverId)
    {
        if (serverId == null) return new BadRequestResult();

        var gameServer = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

        if (gameServer == null) return new NotFoundResult();

        var gameServerDto = new GameServerDto
        {
            Id = gameServer.ServerId,
            Title = gameServer.Title,
            GameType = gameServer.GameType.ToString(),
            Hostname = gameServer.Hostname,
            QueryPort = gameServer.QueryPort,
            FtpHostname = gameServer.FtpHostname,
            FtpUsername = gameServer.FtpUsername,
            FtpPassword = gameServer.FtpPassword,
            RconPassword = gameServer.RconPassword,
            ShowOnBannerServerList = gameServer.ShowOnBannerServerList,
            HtmlBanner = gameServer.HtmlBanner,
            BannerServerListPosition = gameServer.BannerServerListPosition,
            ShowOnPortalServerList = gameServer.ShowOnPortalServerList,
            ShowChatLog = gameServer.ShowChatLog
        };

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

        var gameServerToUpdate = await Context.GameServers.SingleOrDefaultAsync(gs => gs.ServerId == serverId);

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

        var banFileMonitor = new BanFileMonitors
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

        var result = new BanFileMonitorDto
        {
            BanFileMonitorId = banFileMonitor.BanFileMonitorId,
            FilePath = banFileMonitor.FilePath,
            RemoteFileSize = banFileMonitor.RemoteFileSize,
            LastSync = banFileMonitor.LastSync,
            ServerId = banFileMonitor.GameServerServerId,
            GameType = banFileMonitor.GameServerServer.GameType.ToString()
        };

        return new OkObjectResult(result);
    }
}