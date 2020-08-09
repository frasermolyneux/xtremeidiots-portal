using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.BanFileMonitors.Extensions;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.FileMonitors.Extensions;
using XI.Portal.Auth.GameServerStatus.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessStatus)]
    public class StatusController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly ILogFileMonitorStateRepository _logFileMonitorStateRepository;

        public StatusController(
            IBanFileMonitorsRepository banFileMonitorsRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            ILogFileMonitorStateRepository logFileMonitorStateRepository)
        {
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _logFileMonitorStateRepository = logFileMonitorStateRepository ?? throw new ArgumentNullException(nameof(logFileMonitorStateRepository));
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
            var filterModel = new FileMonitorFilterModel().ApplyAuth(User);

            var logFileMonitorStateDtos = await _logFileMonitorStateRepository.GetLogFileMonitorStates(filterModel);

            return View(logFileMonitorStateDtos);
        }

        public async Task<IActionResult> GameServerStatus()
        {
            var filterModel = new GameServerStatusFilterModel().ApplyAuthForGameServerStatus(User);

            var statusModel = await _gameServerStatusRepository.GetAllStatusModels(filterModel, TimeSpan.Zero);
            return View(statusModel);
        }
    }
}