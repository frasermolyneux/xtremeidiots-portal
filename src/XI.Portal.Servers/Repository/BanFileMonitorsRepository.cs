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
    public class BanFileMonitorsRepository : IBanFileMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IBanFileMonitorsRepositoryOptions _options;

        public BanFileMonitorsRepository(IBanFileMonitorsRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<List<BanFileMonitors>> GetBanFileMonitors(ClaimsPrincipal user)
        {
            return await _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(user)
                .Include(b => b.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition).ToListAsync();
        }

        public async Task<BanFileMonitors> GetBanFileMonitor(Guid? id, ClaimsPrincipal user)
        {
            return await _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(user)
                .Include(b => b.GameServerServer)
                .FirstOrDefaultAsync(m => m.BanFileMonitorId == id);
        }

        public async Task CreateBanFileMonitor(BanFileMonitors model)
        {
            model.BanFileMonitorId = Guid.NewGuid();
            model.LastSync = DateTime.UtcNow;

            _legacyContext.Add(model);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateBanFileMonitor(Guid? id, BanFileMonitors model, ClaimsPrincipal user)
        {
            var storedModel = await GetBanFileMonitor(id, user);
            storedModel.FilePath = model.FilePath;

            _legacyContext.Update(storedModel);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> BanFileMonitorExists(Guid id, ClaimsPrincipal user)
        {
            return await _legacyContext.BanFileMonitors.ApplyAuthPolicies(user).AnyAsync(e => e.BanFileMonitorId == id);
        }

        public async Task RemoveBanFileMonitor(Guid id, ClaimsPrincipal user)
        {
            var model = await GetBanFileMonitor(id, user);

            _legacyContext.BanFileMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }
    }
}