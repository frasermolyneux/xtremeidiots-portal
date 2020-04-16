using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Servers.Repository;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.CanAccessLiveRcon)]
    public class LiveRconController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<LiveRconController> _logger;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin};

        public LiveRconController(ILogger<LiveRconController> logger,
            IGameServersRepository gameServersRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
        }

        public async Task<IActionResult> Index()
        {
            var servers = (await _gameServersRepository.GetGameServers(User, _requiredClaims))
                .Where(server => !string.IsNullOrWhiteSpace(server.RconPassword));

            return View(servers);
        }

        public async Task<IActionResult> ViewRcon(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServersRepository.GetGameServer(id, User, _requiredClaims);

            return View(model);
        }

        public async Task<IActionResult> GetRconPlayers(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _gameServersRepository.GetServerStatus(id, User, _requiredClaims);

            return Json(new
            {
                data = model.Players
            });
        }
    }
}