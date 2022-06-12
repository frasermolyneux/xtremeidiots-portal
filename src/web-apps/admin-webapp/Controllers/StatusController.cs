using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessStatus)]
    public class StatusController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        public StatusController(
            IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient;
        }

        public async Task<IActionResult> BanFileStatus()
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition);

            var models = new List<EditBanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitorsApiResponse.Result.Entries)
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(banFileMonitor.ServerId);

                models.Add(new EditBanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                    FilePath = banFileMonitor.FilePath,
                    RemoteFileSize = banFileMonitor.RemoteFileSize,
                    LastSync = banFileMonitor.LastSync,
                    ServerId = banFileMonitor.ServerId,
                    GameServer = gameServerApiResponse.Result
                });
            }

            return View(models);
        }
    }
}