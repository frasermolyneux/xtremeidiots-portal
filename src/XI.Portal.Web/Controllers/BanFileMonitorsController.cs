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
using XI.Portal.Servers.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.ServersManagement)]
    public class BanFileMonitorsController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<BanFileMonitorsController> _logger;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin};

        public BanFileMonitorsController(IBanFileMonitorsRepository banFileMonitorsRepository, IGameServersRepository gameServersRepository, ILogger<BanFileMonitorsController> logger)
        {
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var models = await _banFileMonitorsRepository.GetBanFileMonitors(User, _requiredClaims);

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _banFileMonitorsRepository.GetBanFileMonitor(id, User, _requiredClaims);

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
            [Bind("FilePath,GameServerServerId")] BanFileMonitors model)
        {
            if (!User.HasGameClaim(model.GameServerServer.GameType, _requiredClaims)) return Unauthorized();

            if (ModelState.IsValid)
            {
                await _banFileMonitorsRepository.CreateBanFileMonitor(model);

                _logger.LogInformation(EventIds.Management, "User {User} has created a new ban file monitor with Id {Id}", User.Username(), model.BanFileMonitorId);

                TempData["Success"] = "A new Ban File Monitor has been successfully created";
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

            var model = await _banFileMonitorsRepository.GetBanFileMonitor(id, User, _requiredClaims);

            if (model == null) return NotFound();

            ViewData["GameServerServerId"] = new SelectList(
                await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind("BanFileMonitorId,FilePath,GameServerServerId")]
            BanFileMonitors model)
        {
            if (id != model.BanFileMonitorId) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    await _banFileMonitorsRepository.UpdateBanFileMonitor(id, model, User, _requiredClaims);

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a ban file monitor with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The Ban File Monitor has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await BanFileMonitorsExists(model.BanFileMonitorId))
                        return NotFound();
                    throw;
                }

            ViewData["GameServerServerId"] = new SelectList(
                await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title",
                model.GameServerServerId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _banFileMonitorsRepository.GetBanFileMonitor(id, User, _requiredClaims);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _banFileMonitorsRepository.RemoveBanFileMonitor(id, User, _requiredClaims);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a ban file monitor with Id {Id}", User.Username(), id);

            TempData["Success"] = "The Ban File Monitor has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> BanFileMonitorsExists(Guid id)
        {
            return await _banFileMonitorsRepository.BanFileMonitorExists(id, User, _requiredClaims);
        }
    }
}