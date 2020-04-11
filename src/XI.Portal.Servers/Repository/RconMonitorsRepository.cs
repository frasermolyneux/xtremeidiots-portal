using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Extensions;

namespace XI.Portal.Servers.Repository
{
    public class RconMonitorsRepository : IRconMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IRconMonitorsRepositoryOptions _options;

        public RconMonitorsRepository(IRconMonitorsRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<List<RconMonitors>> GetRconMonitors(ClaimsPrincipal user)
        {
            return await _legacyContext.RconMonitors
                .ApplyAuthPolicies(user)
                .Include(r => r.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition).ToListAsync();
        }

        public async Task<RconMonitors> GetRconMonitor(Guid? id, ClaimsPrincipal user)
        {
            return await _legacyContext.RconMonitors
                .ApplyAuthPolicies(user)
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

        public async Task UpdateRconMonitor(Guid? id, RconMonitors model, ClaimsPrincipal user)
        {
            var storedModel = await GetRconMonitor(id, user);
            storedModel.MonitorMapRotation = model.MonitorMapRotation;
            storedModel.MonitorPlayers = model.MonitorPlayers;
            storedModel.MonitorPlayerIps = model.MonitorPlayerIps;

            _legacyContext.Update(storedModel);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> RconMonitorExists(Guid id, ClaimsPrincipal user)
        {
            return await _legacyContext.RconMonitors.ApplyAuthPolicies(user).AnyAsync(e => e.RconMonitorId == id);
        }

        public async Task RemoveRconMonitor(Guid id, ClaimsPrincipal user)
        {
            var model = await GetRconMonitor(id, user);

            _legacyContext.RconMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }
    }
}