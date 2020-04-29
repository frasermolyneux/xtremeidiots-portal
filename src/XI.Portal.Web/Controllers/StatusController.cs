using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.ViewServiceStatus)]
    public class StatusController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IFileMonitorsRepository _fileMonitorsRepository;
        private readonly IRconMonitorsRepository _rconMonitorsRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;

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
            //var statusModel = await _banFileMonitorsRepository.GetStatusModel(User, _requiredClaims);
            return View();
        }

        public async Task<IActionResult> LogFileStatus()
        {
            var statusModel = await _fileMonitorsRepository.GetStatusModel(User, _requiredClaims);
            return View(statusModel);
        }

        public async Task<IActionResult> RconStatus()
        {
            var statusModel = await _rconMonitorsRepository.GetStatusModel(User, _requiredClaims);
            return View(statusModel);
        }

        public async Task<IActionResult> GameServerStatus()
        {
            var statusModel = await _gameServerStatusRepository.GetAllStatusModels(User, _requiredClaims, TimeSpan.Zero);
            return View(statusModel);
        }
    }
}