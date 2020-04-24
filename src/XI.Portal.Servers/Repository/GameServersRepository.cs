using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Repository
{
    public class GameServersRepository : IGameServersRepository
    {
        private readonly IGameServerClientFactory _gameServerClientFactory;
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<GameServersRepository> _logger;
        private readonly IGameServersRepositoryOptions _options;

        public GameServersRepository(
            ILogger<GameServersRepository> logger,
            IGameServersRepositoryOptions options,
            LegacyPortalContext legacyContext,
            IGameServerClientFactory gameServerClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _gameServerClientFactory = gameServerClientFactory ?? throw new ArgumentNullException(nameof(gameServerClientFactory));
        }

        public async Task<List<GameServerDto>> GetGameServers(ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var servers = await _legacyContext.GameServers
                .ApplyAuth(user, requiredClaims)
                .OrderBy(server => server.BannerServerListPosition)
                .ToListAsync();

            var results = servers.Select(server => new GameServerDto
            {
                ServerId = server.ServerId,
                Title = server.Title,
                GameType = server.GameType,
                Hostname = server.Hostname,
                QueryPort = server.QueryPort,
                FtpHostname = server.FtpHostname,
                FtpUsername = server.FtpUsername,
                FtpPassword = server.FtpPassword,
                RconPassword = server.RconPassword,
                ShowOnBannerServerList = server.ShowOnBannerServerList,
                BannerServerListPosition = server.BannerServerListPosition,
                HtmlBanner = server.HtmlBanner,
                ShowOnPortalServerList = server.ShowOnPortalServerList,
                ShowChatLog = server.ShowChatLog
            }).ToList();

            return results;
        }

        public async Task<GameServerDto> GetGameServer(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var server = await _legacyContext.GameServers
                .ApplyAuth(user, requiredClaims)
                .FirstOrDefaultAsync(m => m.ServerId == id);

            var gameServerDto = new GameServerDto
            {
                ServerId = server.ServerId,
                Title = server.Title,
                GameType = server.GameType,
                Hostname = server.Hostname,
                QueryPort = server.QueryPort,
                FtpHostname = server.FtpHostname,
                FtpUsername = server.FtpUsername,
                FtpPassword = server.FtpPassword,
                RconPassword = server.RconPassword,
                ShowOnBannerServerList = server.ShowOnBannerServerList,
                BannerServerListPosition = server.BannerServerListPosition,
                HtmlBanner = server.HtmlBanner,
                ShowOnPortalServerList = server.ShowOnPortalServerList,
                ShowChatLog = server.ShowChatLog
            };

            return gameServerDto;
        }

        public async Task CreateGameServer(GameServerDto model)
        {
            var server = new GameServers
            {
                ServerId = Guid.NewGuid(),
                Title = model.Title,
                GameType = model.GameType,
                Hostname = model.Hostname,
                QueryPort = model.QueryPort,
                FtpHostname = model.FtpHostname,
                FtpUsername = model.FtpUsername,
                FtpPassword = model.FtpPassword,
                RconPassword = model.RconPassword,
                ShowOnBannerServerList = model.ShowOnBannerServerList,
                BannerServerListPosition = model.BannerServerListPosition,
                HtmlBanner = model.HtmlBanner,
                ShowOnPortalServerList = model.ShowOnPortalServerList,
                ShowChatLog = model.ShowChatLog,
#pragma warning disable 618 // Required to prevent SQL error
                LiveLastUpdated = DateTime.UtcNow
#pragma warning restore 618
            };

            _legacyContext.Add(server);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateGameServer(Guid? id, GameServerDto model, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var server = await _legacyContext.GameServers
                .ApplyAuth(user, requiredClaims)
                .FirstOrDefaultAsync(m => m.ServerId == id);

            server.Title = model.Title;
            server.Hostname = model.Hostname;
            server.QueryPort = model.QueryPort;

            if (user.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
            {
                server.FtpHostname = model.FtpHostname;
                server.FtpUsername = model.FtpUsername;
                server.FtpPassword = model.FtpPassword;
            }

            server.RconPassword = model.RconPassword;
            server.ShowOnBannerServerList = model.ShowOnBannerServerList;
            server.BannerServerListPosition = model.BannerServerListPosition;
            server.HtmlBanner = model.HtmlBanner;
            server.ShowOnPortalServerList = model.ShowOnPortalServerList;
            server.ShowChatLog = model.ShowChatLog;

            _legacyContext.Update(server);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> GameServerExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.GameServers.ApplyAuth(user, requiredClaims).AnyAsync(e => e.ServerId == id);
        }

        public async Task RemoveGameServer(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var server = await _legacyContext.GameServers
                .ApplyAuth(user, requiredClaims)
                .FirstOrDefaultAsync(m => m.ServerId == id);

            _legacyContext.GameServers.Remove(server);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<List<GameServerStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        {
            var results = new List<GameServerStatusViewModel>();

            var serverMonitors = (await GetGameServers(user, requiredClaims)).Where(server => server.ShowOnBannerServerList && server.GameType != GameType.Unknown);

            foreach (var serverMonitor in serverMonitors)
                try
                {
                    var gameServerStatusHelper = _gameServerClientFactory.GetGameServerStatusHelper(serverMonitor.GameType, serverMonitor.ServerId, serverMonitor.Hostname, serverMonitor.QueryPort, serverMonitor.RconPassword);
                    var result = await gameServerStatusHelper.GetServerStatus();

                    var errorMessage = string.Empty;

                    //if (serverMonitor.LiveLastUpdated < DateTime.UtcNow.AddMinutes(-15))
                    //    errorMessage = "ERROR - The server has not been directly queried for more than 15 minutes";

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

        public async Task<List<string>> GetGameServerBanners()
        {
            return await _legacyContext.GameServers
                .OrderBy(server => server.BannerServerListPosition)
                .Where(server => server.ShowOnBannerServerList)
                .Select(server => server.HtmlBanner)
                .ToListAsync();
        }
    }
}