using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Models;
using XI.Servers.Factories;
using XI.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class GameServersRepository : IGameServersRepository
    {
        private readonly IGameServerStatusHelperFactory _gameServerStatusHelperFactory;
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<GameServersRepository> _logger;
        private readonly IGameServersRepositoryOptions _options;

        public GameServersRepository(
            ILogger<GameServersRepository> logger,
            IGameServersRepositoryOptions options,
            LegacyPortalContext legacyContext,
            IGameServerStatusHelperFactory gameServerStatusHelperFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _gameServerStatusHelperFactory = gameServerStatusHelperFactory ?? throw new ArgumentNullException(nameof(gameServerStatusHelperFactory));
        }

        public async Task<List<GameServers>> GetGameServers(ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.GameServers.ApplyAuthPolicies(user, requiredClaims).OrderBy(server => server.BannerServerListPosition).ToListAsync();
        }

        public async Task<GameServers> GetGameServer(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.GameServers
                .ApplyAuthPolicies(user, requiredClaims)
                .FirstOrDefaultAsync(m => m.ServerId == id);
        }

        public async Task CreateGameServer(GameServers model)
        {
            model.ServerId = Guid.NewGuid();
            model.LiveLastUpdated = DateTime.UtcNow;

            _legacyContext.Add(model);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateGameServer(Guid? id, GameServers model, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var storedModel = await GetGameServer(id, user, requiredClaims);
            storedModel.Title = model.Title;
            storedModel.Hostname = model.Hostname;
            storedModel.QueryPort = model.QueryPort;
            storedModel.FtpHostname = model.FtpHostname;
            storedModel.FtpUsername = model.FtpUsername;
            storedModel.FtpPassword = model.FtpPassword;
            storedModel.RconPassword = model.RconPassword;
            storedModel.ShowOnBannerServerList = model.ShowOnBannerServerList;
            storedModel.BannerServerListPosition = model.BannerServerListPosition;
            storedModel.HtmlBanner = model.HtmlBanner;
            storedModel.ShowOnPortalServerList = model.ShowOnPortalServerList;
            storedModel.ShowChatLog = model.ShowChatLog;

            _legacyContext.Update(storedModel);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> GameServerExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.GameServers.ApplyAuthPolicies(user, requiredClaims).AnyAsync(e => e.ServerId == id);
        }

        public async Task RemoveGameServer(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var model = await GetGameServer(id, user, requiredClaims);

            _legacyContext.GameServers.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<List<GameServerStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        {
            var results = new List<GameServerStatusViewModel>();

            var serverMonitors = (await GetGameServers(user, requiredClaims)).Where(server => server.ShowOnBannerServerList && server.GameType != GameType.Unknown);

            foreach (var serverMonitor in serverMonitors)
                try
                {
                    var gameServerStatusHelper = _gameServerStatusHelperFactory.GetGameServerStatusHelper(serverMonitor.GameType, serverMonitor.Title, serverMonitor.Hostname, serverMonitor.QueryPort, serverMonitor.RconPassword);
                    var result = await gameServerStatusHelper.GetServerStatus();

                    var errorMessage = string.Empty;

                    if (serverMonitor.LiveLastUpdated < DateTime.UtcNow.AddMinutes(-15))
                        errorMessage = "ERROR - The server has not been directly queried for more than 15 minutes";

                    if (string.IsNullOrWhiteSpace(result.Map))
                        errorMessage = "ERROR - The current map could not be retrieved from the direct query";

                    results.Add(new GameServerStatusViewModel
                    {
                        GameServer = serverMonitor,
                        ErrorMessage = errorMessage,
                        Map = result.Map,
                        Mod = result.Mod,
                        PlayerCount = result.PlayerCount
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new GameServerStatusViewModel
                    {
                        GameServer = serverMonitor,
                        ErrorMessage = ex.Message
                    });
                }

            return results;
        }

        public async Task<IGameServerStatus> GetServerStatus(Guid? id, ClaimsPrincipal user, string[] requiredClaims)
        {
            var model = await GetGameServer(id, user, requiredClaims);

            var gameServerStatusHelper = _gameServerStatusHelperFactory.GetGameServerStatusHelper(model.GameType, model.Title, model.Hostname, model.QueryPort, model.RconPassword);
            var result = await gameServerStatusHelper.GetServerStatus();
            return result;
        }

        //public async Task<GameServerRconStatusViewModel> GetRconStatusModel(Guid? id, ClaimsPrincipal user, string[] requiredClaims)
        //{
        //    var model = await GetGameServer(id, user, requiredClaims);

        //    if (!model.ShowOnBannerServerList || model.GameType == GameType.Unknown)
        //        return null;

        //    try
        //    {
        //        var gameServerState = _gameServerStateHelperFactory.CreateInstance(model.GameType, model.Title, model.Hostname, model.QueryPort, model.RconPassword);
        //        await gameServerState.RefreshGameServerState();
        //        //var rconClient = _rconClientFactory.CreateInstance(model.GameType, model.Title, model.Hostname, model.QueryPort, model.RconPassword);
        //        //var rconPlayers = rconClient.GetPlayers();

        //        return new GameServerRconStatusViewModel
        //        {
        //            GameServer = model,
        //            Players = gameServerState.Players
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting rcon status model for server");
        //        return null;
        //    }
        //}

        //public async Task<List<GameServerStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        //{
        //    var results = new List<GameServerStatusViewModel>();

        //    var serverMonitors = (await GetGameServers(user, requiredClaims)).Where(server => server.ShowOnBannerServerList && server.GameType != GameType.Unknown);

        //    foreach (var serverMonitor in serverMonitors)
        //        try
        //        {
        //            var gameServerClient = _gameServerClientFactory.CreateInstance(serverMonitor.GameType, serverMonitor.Hostname, serverMonitor.QueryPort);
        //            var serverStatus = await gameServerClient.GetServerStatus();

        //            var errorMessage = string.Empty;

        //            if (serverMonitor.LiveLastUpdated < DateTime.UtcNow.AddMinutes(-15))
        //                errorMessage = "ERROR - The server has not been directly queried for more than 15 minutes";

        //            if (string.IsNullOrWhiteSpace(serverStatus.Map))
        //                errorMessage = "ERROR - The current map could not be retrieved from the direct query";

        //            results.Add(new GameServerStatusViewModel
        //            {
        //                GameServer = serverMonitor,
        //                ErrorMessage = errorMessage,
        //                Map = serverStatus.Map,
        //                Mod = serverStatus.Mod,
        //                PlayerCount = serverStatus.PlayerCount,
        //                GameServerStatus = serverStatus
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            results.Add(new GameServerStatusViewModel
        //            {
        //                GameServer = serverMonitor,
        //                ErrorMessage = ex.Message
        //            });
        //        }

        //    return results;
        //}
    }
}