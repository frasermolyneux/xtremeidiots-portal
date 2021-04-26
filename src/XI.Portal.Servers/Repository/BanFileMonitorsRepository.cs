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
    public class BanFileMonitorsRepository : IBanFileMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;

        public BanFileMonitorsRepository(LegacyPortalContext legacyContext)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
        }

        public async Task<int> GetBanFileMonitorsCount(BanFileMonitorFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            return await _legacyContext.BanFileMonitors.ApplyFilter(filterModel).CountAsync();
        }

        public async Task<List<BanFileMonitorDto>> GetBanFileMonitors(BanFileMonitorFilterModel filterModel)
        {
            if (filterModel == null) throw new ArgumentNullException(nameof(filterModel));

            var banFileMonitors = await _legacyContext.BanFileMonitors
                .ApplyFilter(filterModel)
                .ToListAsync();

            var models = banFileMonitors.Select(bfm => bfm.ToDto()).ToList();

            return models;
        }

        public async Task<BanFileMonitorDto> GetBanFileMonitor(Guid banFileMonitorId)
        {
            var banFileMonitor = await _legacyContext.BanFileMonitors
                .Include(bfm => bfm.GameServerServer)
                .SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorId);

            return banFileMonitor?.ToDto();
        }

        public async Task CreateBanFileMonitor(BanFileMonitorDto banFileMonitorDto)
        {
            if (banFileMonitorDto == null) throw new ArgumentNullException(nameof(banFileMonitorDto));

            var server = await _legacyContext.GameServers.SingleOrDefaultAsync(s => s.ServerId == banFileMonitorDto.ServerId);

            if (server == null)
                throw new NullReferenceException(nameof(server));

            var banFileMonitor = new BanFileMonitors
            {
                BanFileMonitorId = Guid.NewGuid(),
                FilePath = banFileMonitorDto.FilePath,
                //RemoteFileSize = banFileMonitorDto.RemoteFileSize,
                LastSync = DateTime.UtcNow.AddHours(-4),
                //LastError = string.Empty,
                GameServerServer = server
            };

            _legacyContext.BanFileMonitors.Add(banFileMonitor);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task UpdateBanFileMonitor(BanFileMonitorDto banFileMonitorDto)
        {
            if (banFileMonitorDto == null) throw new ArgumentNullException(nameof(banFileMonitorDto));

            var banFileMonitor = await _legacyContext.BanFileMonitors.SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorDto.BanFileMonitorId);

            if (banFileMonitor == null)
                throw new NullReferenceException(nameof(banFileMonitor));

            banFileMonitor.FilePath = banFileMonitorDto.FilePath;
            banFileMonitor.RemoteFileSize = banFileMonitorDto.RemoteFileSize;
            banFileMonitor.LastSync = banFileMonitorDto.LastSync;

            await _legacyContext.SaveChangesAsync();
        }

        public async Task DeleteBanFileMonitor(Guid banFileMonitorId)
        {
            var banFileMonitor = await _legacyContext.BanFileMonitors
                .SingleOrDefaultAsync(bfm => bfm.BanFileMonitorId == banFileMonitorId);

            if (banFileMonitor == null)
                throw new NullReferenceException(nameof(banFileMonitor));

            _legacyContext.Remove(banFileMonitor);
            await _legacyContext.SaveChangesAsync();
        }

        //public async Task<List<BanFileMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        //{
        //    var results = new List<BanFileMonitorStatusViewModel>();

        //    var banFileMonitors = await GetBanFileMonitors(user, requiredClaims);

        //    foreach (var banFileMonitor in banFileMonitors)
        //        try
        //        {
        //            var fileSize = _ftpHelper.GetFileSize(banFileMonitor.GameServerServer.FtpHostname, banFileMonitor.FilePath, banFileMonitor.GameServerServer.FtpUsername, banFileMonitor.GameServerServer.FtpPassword);
        //            var lastModified = _ftpHelper.GetLastModified(banFileMonitor.GameServerServer.FtpHostname, banFileMonitor.FilePath, banFileMonitor.GameServerServer.FtpUsername, banFileMonitor.GameServerServer.FtpPassword);

        //            var lastBans = await _adminActionsRepository.GetAdminActions(new AdminActionsFilterModel
        //            {
        //                Filter = AdminActionsFilterModel.FilterType.ActiveBans,
        //                GameType = banFileMonitor.GameServerServer.GameType,
        //                Order = AdminActionsFilterModel.OrderBy.CreatedDesc,
        //                SkipEntries = 0,
        //                TakeEntries = 1
        //            });

        //            var lastBan = lastBans.FirstOrDefault();
        //            var lastGameBan = DateTime.MinValue;

        //            var errorMessage = string.Empty;
        //            var warningMessage = string.Empty;

        //            if (lastBan != null)
        //            {
        //                lastGameBan = lastBan.Created;

        //                if (lastGameBan >= lastModified)
        //                    errorMessage = "OUT OF SYNC - There are new portal bans that have not been applied.";

        //                if (fileSize != banFileMonitor.RemoteFileSize)
        //                    errorMessage = "OUT OF SYNC - The remote file size does not match the last sync size.";
        //            }

        //            if (banFileMonitor.LastSync < DateTime.UtcNow.AddMinutes(-30))
        //                warningMessage = "WARNING - It has been more than 30 minutes since the server had a sync check";

        //            results.Add(new BanFileMonitorStatusViewModel
        //            {
        //                BanFileMonitor = banFileMonitor,
        //                GameServer = banFileMonitor.GameServerServer,
        //                FileSize = fileSize,
        //                LastModified = lastModified,
        //                LastGameBan = lastGameBan,
        //                ErrorMessage = errorMessage,
        //                WarningMessage = warningMessage
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            results.Add(new BanFileMonitorStatusViewModel
        //            {
        //                BanFileMonitor = banFileMonitor,
        //                GameServer = banFileMonitor.GameServerServer,
        //                ErrorMessage = ex.Message
        //            });
        //        }

        //    return results;
        //}
    }
}