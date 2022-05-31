using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
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

            var banFileMonitorDtos = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 0, "BannerServerListPosition");

            var models = new List<BanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitorDtos)
            {
                var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitor.ServerId);

                models.Add(new BanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                    FilePath = banFileMonitor.FilePath,
                    RemoteFileSize = banFileMonitor.RemoteFileSize,
                    LastSync = banFileMonitor.LastSync,
                    ServerId = banFileMonitor.ServerId,
                    GameServer = gameServerDto
                });
            }

            return View(models);
        }
    }
}