using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.BanFileMonitors.Extensions;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.ServersManagement)]
    public class BanFileMonitorsController : Controller
    {
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<BanFileMonitorsController> _logger;
        private readonly IAuthorizationService _authorizationService;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin};

        public BanFileMonitorsController(
            ILogger<BanFileMonitorsController> logger,
            IAuthorizationService authorizationService,
            IBanFileMonitorsRepository banFileMonitorsRepository, 
            IGameServersRepository gameServersRepository)
        {
            _banFileMonitorsRepository = banFileMonitorsRepository ?? throw new ArgumentNullException(nameof(banFileMonitorsRepository));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filterModel = new BanFileMonitorFilterModel().ApplyAuth(User);
            var banFileMonitors = await _banFileMonitorsRepository.GetBanFileMonitors(filterModel);

            return View(banFileMonitors);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["GameServerServerId"] = new SelectList(await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BanFileMonitorDto model)
        {
            var server = await _gameServersRepository.GetGameServer(model.ServerId, User, _requiredClaims);

            if (server == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["GameServerServerId"] = new SelectList(await _gameServersRepository.GetGameServers(User, _requiredClaims), "ServerId", "Title");
                return View(model);
            }







            if (!User.HasGameClaim(server.GameType, _requiredClaims)) return Unauthorized();

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
        public async Task<IActionResult> Details(Guid id)
        {





            if (id == null) return NotFound();

            var model = await _banFileMonitorsRepository.GetBanFileMonitor(id, User, _requiredClaims);

            if (model == null) return NotFound();

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
            await _banFileMonitorsRepository.DeleteBanFileMonitor(id, User, _requiredClaims);

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