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
using XI.Portal.Servers.Helpers;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class FileMonitorsRepository : IFileMonitorsRepository
    {
        private readonly IFtpHelper _ftpHelper;
        private readonly LegacyPortalContext _legacyContext;
        private readonly IFileMonitorsRepositoryOptions _options;

        public FileMonitorsRepository(IFileMonitorsRepositoryOptions options, LegacyPortalContext legacyContext, IFtpHelper ftpHelper)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _ftpHelper = ftpHelper ?? throw new ArgumentNullException(nameof(ftpHelper));
        }

        public async Task<List<FileMonitors>> GetFileMonitors(ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.FileMonitors
                .ApplyAuthPolicies(user, requiredClaims)
                .Include(f => f.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition).ToListAsync();
        }

        public async Task<FileMonitors> GetFileMonitor(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.FileMonitors
                .ApplyAuthPolicies(user, requiredClaims)
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

        public async Task UpdateFileMonitor(Guid? id, FileMonitors model, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var storedModel = await GetFileMonitor(id, user, requiredClaims);
            storedModel.FilePath = model.FilePath;

            _legacyContext.Update(storedModel);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> FileMonitorExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.FileMonitors.ApplyAuthPolicies(user, requiredClaims).AnyAsync(e => e.FileMonitorId == id);
        }

        public async Task RemoveFileMonitor(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var model = await GetFileMonitor(id, user, requiredClaims);

            _legacyContext.FileMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<List<FileMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        {
            var results = new List<FileMonitorStatusViewModel>();

            var fileMonitors = await GetFileMonitors(user, requiredClaims);

            foreach (var fileMonitor in fileMonitors)
                try
                {
                    var fileSize = _ftpHelper.GetFileSize(fileMonitor.GameServerServer.Hostname, fileMonitor.FilePath, fileMonitor.GameServerServer.FtpUsername, fileMonitor.GameServerServer.FtpPassword);
                    var lastModified = _ftpHelper.GetLastModified(fileMonitor.GameServerServer.Hostname, fileMonitor.FilePath, fileMonitor.GameServerServer.FtpUsername, fileMonitor.GameServerServer.FtpPassword);

                    var errorMessage = string.Empty;
                    var warningMessage = string.Empty;

                    if (lastModified < DateTime.Now.AddHours(-1))
                        errorMessage = "INVESTIGATE - The log file has not been modified in over 1 hour.";

                    if (fileMonitor.LastRead < DateTime.UtcNow.AddMinutes(-15))
                        warningMessage = "WARNING - The file has not been read in the past 15 minutes";

                    if (fileMonitor.LastRead < DateTime.UtcNow.AddMinutes(-30))
                        errorMessage = "ERROR - The file has not been read in the past 30 minutes";

                    results.Add(new FileMonitorStatusViewModel
                    {
                        FileMonitor = fileMonitor,
                        GameServer = fileMonitor.GameServerServer,
                        FileSize = fileSize,
                        LastModified = lastModified,
                        ErrorMessage = errorMessage,
                        WarningMessage = warningMessage
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new FileMonitorStatusViewModel
                    {
                        FileMonitor = fileMonitor,
                        GameServer = fileMonitor.GameServerServer,
                        ErrorMessage = ex.Message
                    });
                }


            return results;
        }
    }
}