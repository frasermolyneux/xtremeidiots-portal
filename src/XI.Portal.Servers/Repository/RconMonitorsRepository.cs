using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Repository
{
    public class RconMonitorsRepository : IRconMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IRconMonitorsRepositoryOptions _options;
        private readonly IRconClientFactory _rconClientFactory;

        public RconMonitorsRepository(IRconMonitorsRepositoryOptions options, LegacyPortalContext legacyContext, IRconClientFactory rconClientFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        public async Task<List<RconMonitors>> GetRconMonitors(ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.RconMonitors
                .ApplyAuthPolicies(user, requiredClaims)
                .Include(r => r.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition).ToListAsync();
        }

        public async Task<RconMonitors> GetRconMonitor(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.RconMonitors
                .ApplyAuthPolicies(user, requiredClaims)
                .Include(r => r.GameServerServer)
                .FirstOrDefaultAsync(m => m.RconMonitorId == id);
        }

        public async Task CreateRconMonitor(RconMonitors model)
        {
            model.RconMonitorId = Guid.NewGuid();
            model.LastUpdated = DateTime.UtcNow;
            model.MapRotationLastUpdated = DateTime.UtcNow;

            _legacyContext.Add(model);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateRconMonitor(Guid? id, RconMonitors model, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var storedModel = await GetRconMonitor(id, user, requiredClaims);
            storedModel.MonitorMapRotation = model.MonitorMapRotation;
            storedModel.MonitorPlayers = model.MonitorPlayers;
            storedModel.MonitorPlayerIps = model.MonitorPlayerIps;

            _legacyContext.Update(storedModel);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> RconMonitorExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.RconMonitors.ApplyAuthPolicies(user, requiredClaims).AnyAsync(e => e.RconMonitorId == id);
        }

        public async Task RemoveRconMonitor(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var model = await GetRconMonitor(id, user, requiredClaims);

            _legacyContext.RconMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<List<RconMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        {
            var results = new List<RconMonitorStatusViewModel>();

            var rconMonitors = await GetRconMonitors(user, requiredClaims);

            foreach (var rconMonitor in rconMonitors)
                try
                {
                    var rconClient = _rconClientFactory.CreateInstance(
                        rconMonitor.GameServerServer.GameType,
                        rconMonitor.GameServerServer.ServerId,
                        rconMonitor.GameServerServer.Hostname,
                        rconMonitor.GameServerServer.QueryPort,
                        rconMonitor.GameServerServer.RconPassword
                    );

                    var commandResult = rconClient.GetPlayers();

                    var errorMessage = string.Empty;

                    if (rconMonitor.LastUpdated < DateTime.UtcNow.AddMinutes(-15))
                        errorMessage = "ERROR - The rcon status has not been updated in the past 15 minutes";


                    results.Add(new RconMonitorStatusViewModel
                    {
                        RconMonitor = rconMonitor,
                        GameServer = rconMonitor.GameServerServer,
                        RconStatusResult = $"Total players: {commandResult.Count}",
                        ErrorMessage = errorMessage
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new RconMonitorStatusViewModel
                    {
                        RconMonitor = rconMonitor,
                        GameServer = rconMonitor.GameServerServer,
                        ErrorMessage = ex.Message
                    });
                }

            return results;
        }
    }
}