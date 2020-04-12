using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Players.Models;
using XI.Portal.Players.Repository;
using XI.Portal.Servers.Configuration;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Helpers;
using XI.Portal.Servers.Models;

namespace XI.Portal.Servers.Repository
{
    public class BanFileMonitorsRepository : IBanFileMonitorsRepository
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly IFtpHelper _ftpHelper;
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IBanFileMonitorsRepositoryOptions _options;

        public BanFileMonitorsRepository(IBanFileMonitorsRepositoryOptions options, LegacyPortalContext legacyContext, IFtpHelper ftpHelper, IAdminActionsRepository adminActionsRepository)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _ftpHelper = ftpHelper ?? throw new ArgumentNullException(nameof(ftpHelper));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
        }

        public async Task<List<BanFileMonitors>> GetBanFileMonitors(ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(user, requiredClaims)
                .Include(b => b.GameServerServer)
                .OrderBy(monitor => monitor.GameServerServer.BannerServerListPosition).ToListAsync();
        }

        public async Task<BanFileMonitors> GetBanFileMonitor(Guid? id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.BanFileMonitors
                .ApplyAuthPolicies(user, requiredClaims)
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

        public async Task UpdateBanFileMonitor(Guid? id, BanFileMonitors model, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var storedModel = await GetBanFileMonitor(id, user, requiredClaims);
            storedModel.FilePath = model.FilePath;

            _legacyContext.Update(storedModel);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<bool> BanFileMonitorExists(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            return await _legacyContext.BanFileMonitors.ApplyAuthPolicies(user, requiredClaims).AnyAsync(e => e.BanFileMonitorId == id);
        }

        public async Task RemoveBanFileMonitor(Guid id, ClaimsPrincipal user, IEnumerable<string> requiredClaims)
        {
            var model = await GetBanFileMonitor(id, user, requiredClaims);

            _legacyContext.BanFileMonitors.Remove(model);
            await _legacyContext.SaveChangesAsync();
        }

        public async Task<List<BanFileMonitorStatusViewModel>> GetStatusModel(ClaimsPrincipal user, string[] requiredClaims)
        {
            var results = new List<BanFileMonitorStatusViewModel>();

            var banFileMonitors = await GetBanFileMonitors(user, requiredClaims);

            foreach (var banFileMonitor in banFileMonitors)
                try
                {
                    var fileSize = _ftpHelper.GetFileSize(banFileMonitor.GameServerServer.FtpHostname, banFileMonitor.FilePath, banFileMonitor.GameServerServer.FtpUsername, banFileMonitor.GameServerServer.FtpPassword);
                    var lastModified = _ftpHelper.GetLastModified(banFileMonitor.GameServerServer.FtpHostname, banFileMonitor.FilePath, banFileMonitor.GameServerServer.FtpUsername, banFileMonitor.GameServerServer.FtpPassword);

                    var lastBans = await _adminActionsRepository.GetAdminActionsList(new AdminActionsFilterModel
                    {
                        Filter = AdminActionsFilterModel.FilterType.ActiveBans,
                        GameType = banFileMonitor.GameServerServer.GameType,
                        Order = AdminActionsFilterModel.OrderBy.CreatedDesc,
                        SkipEntries = 0,
                        TakeEntries = 1
                    });

                    var lastBan = lastBans.FirstOrDefault();
                    var lastGameBan = DateTime.MinValue;

                    var errorMessage = string.Empty;
                    var warningMessage = string.Empty;

                    if (lastBan != null)
                    {
                        lastGameBan = lastBan.Created;

                        if (lastGameBan >= lastModified)
                            errorMessage = "OUT OF SYNC - There are new portal bans that have not been applied.";

                        if (fileSize != banFileMonitor.RemoteFileSize)
                            errorMessage = "OUT OF SYNC - The remote file size does not match the last sync size.";
                    }

                    if (banFileMonitor.LastSync < DateTime.UtcNow.AddMinutes(-30))
                        warningMessage = "WARNING - It has been more than 30 minutes since the server had a sync check";

                    results.Add(new BanFileMonitorStatusViewModel
                    {
                        BanFileMonitor = banFileMonitor,
                        GameServer = banFileMonitor.GameServerServer,
                        FileSize = fileSize,
                        LastModified = lastModified,
                        LastGameBan = lastGameBan,
                        ErrorMessage = errorMessage,
                        WarningMessage = warningMessage
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new BanFileMonitorStatusViewModel
                    {
                        BanFileMonitor = banFileMonitor,
                        GameServer = banFileMonitor.GameServerServer,
                        ErrorMessage = ex.Message
                    });
                }

            return results;
        }
    }
}