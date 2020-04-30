using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class FileMonitorsRepository : IFileMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public FileMonitorsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetFileMonitorsCount(FileMonitorFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            return await _legacyContext.FileMonitors.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<FileMonitorDto>> GetFileMonitors(FileMonitorFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            var fileMonitors = await _legacyContext.FileMonitors
                .ApplyFilter(filterModel)
                .ToListAsync();

            var models = fileMonitors.Select(fm => fm.ToDto()).ToList();

            return models;
        }

        public async Task<FileMonitorDto> GetFileMonitor(Guid fileMonitorId)
        {
            var fileMonitor = await _legacyContext.FileMonitors
                .Include(fm => fm.GameServerServer)
                .SingleOrDefaultAsync(fm => fm.FileMonitorId == fileMonitorId);

            return fileMonitor?.ToDto();
        }

        public async Task CreateFileMonitor(FileMonitorDto fileMonitorDto)
        {
            if (fileMonitorDto == null) throw new ArgumentNullException(nameof(fileMonitorDto));

            var server = await _legacyContext.GameServers.SingleOrDefaultAsync(s => s.ServerId == fileMonitorDto.ServerId);

            if (server == null)
                throw new NullReferenceException(nameof(server));

            var fileMonitor = new FileMonitors
            {
                FileMonitorId = Guid.NewGuid(),
                FilePath = fileMonitorDto.FilePath,
                BytesRead = fileMonitorDto.BytesRead,
                LastRead = fileMonitorDto.LastRead,
                //LastError = string.Empty;
                GameServerServer = server
            };

            _legacyContext.FileMonitors.Add(fileMonitor);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateFileMonitor(FileMonitorDto fileMonitorDto)
        {
            if (fileMonitorDto == null) throw new ArgumentNullException(nameof(fileMonitorDto));

            var fileMonitor = await _legacyContext.FileMonitors.SingleOrDefaultAsync(fm => fm.FileMonitorId == fileMonitorDto.FileMonitorId);

            if (fileMonitor == null)
                throw new NullReferenceException(nameof(fileMonitor));

            fileMonitor.FilePath = fileMonitorDto.FilePath;

            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteFileMonitor(Guid fileMonitorId)
        {
            var fileMonitor = await _legacyContext.FileMonitors
                .SingleOrDefaultAsync(fm => fm.FileMonitorId == fileMonitorId);

            if (fileMonitor == null)
                throw new NullReferenceException(nameof(fileMonitor));

            _legacyContext.Remove(fileMonitor);
            await _legacyContext.SaveChangesAsync();
        }

        //public async Task<List<FileMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        //{
        //    var results = new List<FileMonitorStatusViewModel>();

        //    var fileMonitors = await GetFileMonitors(user, requiredClaims);

        //    foreach (var fileMonitor in fileMonitors)
        //        try
        //        {
        //            var fileSize = _ftpHelper.GetFileSize(fileMonitor.GameServerServer.Hostname, fileMonitor.FilePath, fileMonitor.GameServerServer.FtpUsername, fileMonitor.GameServerServer.FtpPassword);
        //            var lastModified = _ftpHelper.GetLastModified(fileMonitor.GameServerServer.Hostname, fileMonitor.FilePath, fileMonitor.GameServerServer.FtpUsername, fileMonitor.GameServerServer.FtpPassword);

        //            var errorMessage = string.Empty;
        //            var warningMessage = string.Empty;

        //            if (lastModified < DateTime.Now.AddHours(-1))
        //                errorMessage = "INVESTIGATE - The log file has not been modified in over 1 hour.";

        //            if (fileMonitor.LastRead < DateTime.UtcNow.AddMinutes(-15))
        //                warningMessage = "WARNING - The file has not been read in the past 15 minutes";

        //            if (fileMonitor.LastRead < DateTime.UtcNow.AddMinutes(-30))
        //                errorMessage = "ERROR - The file has not been read in the past 30 minutes";

        //            results.Add(new FileMonitorStatusViewModel
        //            {
        //                FileMonitor = fileMonitor,
        //                GameServer = fileMonitor.GameServerServer,
        //                FileSize = fileSize,
        //                LastModified = lastModified,
        //                ErrorMessage = errorMessage,
        //                WarningMessage = warningMessage
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            results.Add(new FileMonitorStatusViewModel
        //            {
        //                FileMonitor = fileMonitor,
        //                GameServer = fileMonitor.GameServerServer,
        //                ErrorMessage = ex.Message
        //            });
        //        }


        //    return results;
        //}
    }
}