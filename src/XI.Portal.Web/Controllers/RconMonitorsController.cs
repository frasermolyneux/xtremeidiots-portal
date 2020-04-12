using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Repository;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.ServersManagement)]
    public class RconMonitorsController : Controller
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<RconMonitorsController> _logger;
        private readonly IRconMonitorsRepository _rconMonitorsRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin};

        public RconMonitorsController(IRconMonitorsRepository rconMonitorsRepository, IGameServersRepository gameServersRepository, ILogger<RconMonitorsController> logger)
        {
            _rconMonitorsRepository = rconMonitorsRepository ?? throw new ArgumentNullException(nameof(rconMonitorsRepository));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var models = await _rconMonitorsRepository.GetRconMonitors(User, _requiredClaims);

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _rconMonitorsRepository.GetRconMonitor(id, User, _requiredClaims);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["GameServerServerId"] = new SelectList(await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MonitorMapRotation,MonitorPlayers,MonitorPlayerIps,GameServerServerId")]
            RconMonitors model)
        {
            if (!User.HasGameClaim(model.GameServerServer.GameType, _requiredClaims)) return Unauthorized();

            if (ModelState.IsValid)
            {
                await _rconMonitorsRepository.CreateRconMonitor(model);

                _logger.LogInformation(EventIds.Management, "User {User} has created a new rcon monitor with Id {Id}", User.Username(), model.RconMonitorId);

                TempData["Success"] = "A new Rcon Monitor has been successfully created";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameServerServerId"] = new SelectList(await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _rconMonitorsRepository.GetRconMonitor(id, User, _requiredClaims);

            if (model == null) return NotFound();

            ViewData["GameServerServerId"] = new SelectList(await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("RconMonitorId,MonitorMapRotation,MonitorPlayers,MonitorPlayerIps,GameServerServerId")]
            RconMonitors model)
        {
            if (id != model.RconMonitorId) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    await _rconMonitorsRepository.UpdateRconMonitor(id, model, User, _requiredClaims);

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a rcon monitor with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The Rcon Monitor has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RconMonitorsExists(model.RconMonitorId))
                        return NotFound();
                    throw;
                }

            ViewData["GameServerServerId"] = new SelectList(await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _rconMonitorsRepository.GetRconMonitor(id, User, _requiredClaims);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _rconMonitorsRepository.RemoveRconMonitor(id, User, _requiredClaims);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a rcon monitor with Id {Id}", User.Username(), id);

            TempData["Success"] = "The Rcon Monitor has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RconMonitorsExists(Guid id)
        {
            return await _rconMonitorsRepository.RconMonitorExists(id, User, _requiredClaims);
        }
    }
}