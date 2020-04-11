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
    public class FileMonitorsRepository : IFileMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IFileMonitorsRepositoryOptions _options;

        public FileMonitorsRepository(IFileMonitorsRepositoryOptions options, LegacyPortalContext legacyContext)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<List<FileMonitors>> GetFileMonitors(ClaimsPrincipal user)
        {
            return await _legacyContext.FileMonitors
                .ApplyAuthPolicies(user)
                .Include(f => f.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition).ToListAsync();
        }

        public async Task<FileMonitors> GetFileMonitor(Guid? id, ClaimsPrincipal user)
        {
            return await _legacyContext.FileMonitors
                .ApplyAuthPolicies(user)
                .Include(f => f.GameServerServer)
                .FirstOrDefaultAsync(m => m.FileMonitorId == id);
        }

        public async Task CreateFileMonitor(FileMonitors model)
        {
            model.FileMonitorId = Guid.NewGuid();
            model.LastRead = DateTime.UtcNow;

            _legacyContext.Add(model);

            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateFileMonitor(Guid? id, FileMonitors model, ClaimsPrincipal user)
        {
            var storedModel = await GetFileMonitor(id, user);
            storedModel.FilePath = model.FilePath;

            _legacyContext.Update(storedModel);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> FileMonitorExists(Guid id, ClaimsPrincipal user)
        {
            return await _legacyContext.FileMonitors.ApplyAuthPolicies(user).AnyAsync(e => e.FileMonitorId == id);
        }

        public async Task RemoveFileMonitor(Guid id, ClaimsPrincipal user)
        {
            var model = await GetFileMonitor(id, user);

            _legacyContext.FileMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }
    }
}