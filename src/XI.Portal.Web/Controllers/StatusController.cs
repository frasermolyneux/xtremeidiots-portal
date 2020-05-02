using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.BanFileMonitors.Extensions;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.FileMonitors.Extensions;
using XI.Portal.Auth.RconMonitors.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.ViewServiceStatus)]
    public class StatusController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IFileMonitorsRepository _fileMonitorsRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly IRconMonitorsRepository _rconMonitorsRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin};

        public StatusController(IBanFileMonitorsRepository banFileMonitorsRepository,
            IFileMonitorsRepository fileMonitorsRepository,
            IRconMonitorsRepository rconMonitorsRepository,
            IGameServerStatusRepository gameServerStatusRepository)
        {
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _fileMonitorsRepository = fileMonitorsRepository ?? throw new ArgumentNullException(nameof(fileMonitorsRepository));
            _rconMonitorsRepository = rconMonitorsRepository ?? throw new ArgumentNullException(nameof(rconMonitorsRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
        }

        public async Task<IActionResult> BanFileStatus()
        {
            var filterModel = new BanFileMonitorFilterModel
            {
                Order = BanFileMonitorFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuth(User);

            var banFileMonitorDtos = await _banFileMonitorsRepository.GetBanFileMonitors(filterModel);
            return View(banFileMonitorDtos);
        }

        public async Task<IActionResult> LogFileStatus()
        {
            var filterModel = new FileMonitorFilterModel
            {
                Order = FileMonitorFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuth(User);

            var fileMonitorDtos = await _fileMonitorsRepository.GetFileMonitors(filterModel);
            return View(fileMonitorDtos);
        }

        public async Task<IActionResult> RconStatus()
        {
            var filterModel = new RconMonitorFilterModel
            {
                Order = RconMonitorFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuth(User);

            var rconMonitorDtos = await _rconMonitorsRepository.GetRconMonitors(filterModel);
            return View(rconMonitorDtos);
        }

        public async Task<IActionResult> GameServerStatus()
        {
            var statusModel = await _gameServerStatusRepository.GetAllStatusModels(User, _requiredClaims, TimeSpan.Zero);
            return View(statusModel);
        }
    }
}