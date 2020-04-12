using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Models;
using XI.Servers;
using XI.Servers.Factories;

namespace XI.Portal.Servers.Repository
{
    public class GameServersRepository : IGameServersRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IGameServerClientFactory _gameServerClientFactory;
        private readonly IGameServersRepositoryOptions _options;

        public GameServersRepository(IGameServersRepositoryOptions options, LegacyPortalContext legacyContext, IGameServerClientFactory gameServerClientFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _gameServerClientFactory = gameServerClientFactory ?? throw new ArgumentNullException(nameof(gameServerClientFactory));
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
                    var gameServerClient = _gameServerClientFactory.CreateInstance(serverMonitor.GameType, serverMonitor.Hostname, serverMonitor.QueryPort);
                    var serverStatus = await gameServerClient.GetServerStatus();

                    var errorMessage = string.Empty;

                    if (serverMonitor.LiveLastUpdated < DateTime.UtcNow.AddMinutes(-15))
                        errorMessage = "ERROR - The server has not been directly queried for more than 15 minutes";

                    if (string.IsNullOrWhiteSpace(serverStatus.Map))
                        errorMessage = "ERROR - The current map could not be retrieved from the direct query";

                    results.Add(new GameServerStatusViewModel
                    {
                        GameServer = serverMonitor,
                        ErrorMessage = errorMessage,
                        Map = serverStatus.Map,
                        Mod = serverStatus.Mod,
                        PlayerCount = serverStatus.PlayerCount
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
    }
}