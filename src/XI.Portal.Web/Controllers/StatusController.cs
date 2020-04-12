using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Repository;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.ViewServiceStatus)]
    public class StatusController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IFileMonitorsRepository _fileMonitorsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IRconMonitorsRepository _rconMonitorsRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};

        public StatusController(IBanFileMonitorsRepository banFileMonitorsRepository, IFileMonitorsRepository fileMonitorsRepository, IRconMonitorsRepository rconMonitorsRepository, IGameServersRepository gameServersRepository)
        {
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _fileMonitorsRepository = fileMonitorsRepository ?? throw new ArgumentNullException(nameof(fileMonitorsRepository));
            _rconMonitorsRepository = rconMonitorsRepository ?? throw new ArgumentNullException(nameof(rconMonitorsRepository));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IActionResult> BanFileStatus()
        {
            var statusModel = await _banFileMonitorsRepository.GetStatusModel(User, _requiredClaims);
            return View(statusModel);
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
            var statusModel = await _gameServersRepository.GetStatusModel(User, _requiredClaims);
            return View(statusModel);
        }
    }
}