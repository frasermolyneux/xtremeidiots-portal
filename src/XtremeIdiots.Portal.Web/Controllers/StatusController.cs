using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition);

            var models = new List<EditBanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitorsApiResponse.Result.Data.Items)
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(banFileMonitor.GameServerId);

                models.Add(new EditBanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                    FilePath = banFileMonitor.FilePath,
                    RemoteFileSize = banFileMonitor.RemoteFileSize,
                    LastSync = banFileMonitor.LastSync,
                    GameServerId = banFileMonitor.GameServerId,
                    GameServer = gameServerApiResponse.Result.Data
                });
            }

            return View(models);
        }
    }
}