using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.BanFileMonitors.Extensions;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.AccessBanFileMonitors)]
    public class BanFileMonitorsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IBanFileMonitorsRepository _banFileMonitorsRepository;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<BanFileMonitorsController> _logger;

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
            var filterModel = new BanFileMonitorFilterModel
            {
                Order = BanFileMonitorFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuth(User);
            var banFileMonitorDtos = await _banFileMonitorsRepository.GetBanFileMonitors(filterModel);

            return View(banFileMonitorDtos);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await AddGameServersViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BanFileMonitorDto model)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(model.ServerId);
            if (gameServerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(model);
            }

            var banFileMonitorDto = new BanFileMonitorDto().WithServerDto(gameServerDto);
            var canCreateBanFileMonitor = await _authorizationService.AuthorizeAsync(User, banFileMonitorDto, XtremeIdiotsPolicy.CreateBanFileMonitor);

            if (!canCreateBanFileMonitor.Succeeded)
                return Unauthorized();

            banFileMonitorDto.FilePath = model.FilePath;

            await _banFileMonitorsRepository.CreateBanFileMonitor(banFileMonitorDto);

            _logger.LogInformation(EventIds.Management, "User {User} has created a new ban file monitor with Id {Id}", User.Username(), banFileMonitorDto.BanFileMonitorId);
            this.AddAlertSuccess($"The ban file monitor has been created for {gameServerDto.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var banFileMonitorDto = await _banFileMonitorsRepository.GetBanFileMonitor(id);
            if (banFileMonitorDto == null) return NotFound();

            var canEditBanFileMonitor = await _authorizationService.AuthorizeAsync(User, banFileMonitorDto, XtremeIdiotsPolicy.ViewBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            return View(banFileMonitorDto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var banFileMonitorDto = await _banFileMonitorsRepository.GetBanFileMonitor(id);
            if (banFileMonitorDto == null) return NotFound();

            var canEditBanFileMonitor = await _authorizationService.AuthorizeAsync(User, banFileMonitorDto, XtremeIdiotsPolicy.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorDto.ServerId);

            return View(banFileMonitorDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BanFileMonitorDto model)
        {
            var banFileMonitorDto = await _banFileMonitorsRepository.GetBanFileMonitor(model.BanFileMonitorId);
            if (banFileMonitorDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(banFileMonitorDto);
            }

            var canEditBanFileMonitor = await _authorizationService.AuthorizeAsync(User, banFileMonitorDto, XtremeIdiotsPolicy.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            banFileMonitorDto.FilePath = model.FilePath;

            await _banFileMonitorsRepository.UpdateBanFileMonitor(banFileMonitorDto);

            _logger.LogInformation(EventIds.Management, "User {User} has updated {BanFileMonitorId} against {ServerId}", User.Username(), banFileMonitorDto.BanFileMonitorId, banFileMonitorDto.ServerId);
            this.AddAlertSuccess($"The ban file monitor has been created for {banFileMonitorDto.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var banFileMonitorDto = await _banFileMonitorsRepository.GetBanFileMonitor(id);
            if (banFileMonitorDto == null) return NotFound();

            var canDeleteBanFileMonitor = await _authorizationService.AuthorizeAsync(User, banFileMonitorDto, XtremeIdiotsPolicy.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorDto.ServerId);

            return View(banFileMonitorDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var banFileMonitorDto = await _banFileMonitorsRepository.GetBanFileMonitor(id);
            if (banFileMonitorDto == null) return NotFound();

            var canDeleteBanFileMonitor = await _authorizationService.AuthorizeAsync(User, banFileMonitorDto, XtremeIdiotsPolicy.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await _banFileMonitorsRepository.DeleteBanFileMonitor(id);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted {BanFileMonitorId} against {ServerId}", User.Username(), banFileMonitorDto.BanFileMonitorId, banFileMonitorDto.ServerId);
            this.AddAlertSuccess($"The ban file monitor has been deleted for {banFileMonitorDto.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuthForBanFileMonitors(User);
            var gameServerDtos = await _gameServersRepository.GetGameServers(filterModel);
            ViewData["GameServers"] = new SelectList(gameServerDtos, "ServerId", "Title", selected);
        }
    }
}