﻿using Microsoft.AspNetCore.Authorization;
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
                    query = query.Where(s => s.ShowOnPortalServerList);
                    break;
                case GameServerFilter.ShowOnBannerServerList:
                    query = query.Where(s => s.ShowOnBannerServerList);
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

        gameServerToUpdate.Title = gameServer.Title;
        gameServerToUpdate.Hostname = gameServer.Hostname;
        gameServerToUpdate.QueryPort = gameServer.QueryPort;
        gameServerToUpdate.FtpHostname = gameServer.FtpHostname;
        gameServerToUpdate.FtpUsername = gameServer.FtpUsername;
        gameServerToUpdate.FtpPassword = gameServer.FtpPassword;
        gameServerToUpdate.RconPassword = gameServer.RconPassword;
        gameServerToUpdate.ShowOnBannerServerList = gameServer.ShowOnBannerServerList;
        gameServerToUpdate.HtmlBanner = gameServer.HtmlBanner;
        gameServerToUpdate.BannerServerListPosition = gameServer.BannerServerListPosition;
        gameServerToUpdate.ShowOnPortalServerList = gameServer.ShowOnPortalServerList;
        gameServerToUpdate.ShowChatLog = gameServer.ShowChatLog;

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
}