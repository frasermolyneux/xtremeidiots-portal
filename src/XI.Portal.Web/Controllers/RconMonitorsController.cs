using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Auth.RconMonitors.Extensions;
using XI.Portal.Servers.Dto;
using XI.Portal.Servers.Extensions;
using XI.Portal.Servers.Interfaces;
using XI.Portal.Servers.Models;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessRconMonitors)]
    public class RconMonitorsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IGameServersRepository _gameServersRepository;
        private readonly ILogger<RconMonitorsController> _logger;
        private readonly IRconMonitorsRepository _rconMonitorsRepository;

        private readonly string[] _requiredClaims = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin};

        public RconMonitorsController(
            ILogger<RconMonitorsController> logger,
            IAuthorizationService authorizationService,
            IRconMonitorsRepository rconMonitorsRepository,
            IGameServersRepository gameServersRepository)
        {
            _rconMonitorsRepository = rconMonitorsRepository ?? throw new ArgumentNullException(nameof(rconMonitorsRepository));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var filterModel = new RconMonitorFilterModel
            {
                Order = RconMonitorFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuth(User);
            var rconMonitorsDtos = await _rconMonitorsRepository.GetRconMonitors(filterModel);

            return View(rconMonitorsDtos);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await AddGameServersViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RconMonitorDto model)
        {
            var gameServerDto = await _gameServersRepository.GetGameServer(model.ServerId);
            if (gameServerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(model);
            }

            var rconMonitorDto = new RconMonitorDto().WithServerDto(gameServerDto);
            var canCreateRconMonitor = await _authorizationService.AuthorizeAsync(User, rconMonitorDto, AuthPolicies.CreateRconMonitor);

            if (!canCreateRconMonitor.Succeeded)
                return Unauthorized();

            rconMonitorDto.MonitorMapRotation = model.MonitorMapRotation;
            rconMonitorDto.MonitorPlayers = model.MonitorPlayers;
            rconMonitorDto.MonitorPlayerIps = model.MonitorPlayerIps;

            await _rconMonitorsRepository.CreateRconMonitor(rconMonitorDto);

            _logger.LogInformation(EventIds.Management, "User {User} has created a new rcon monitor with Id {Id}", User.Username(), rconMonitorDto.RconMonitorId);
            this.AddAlertSuccess($"The rcon monitor has been created for {gameServerDto.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var rconMonitorDto = await _rconMonitorsRepository.GetRconMonitor(id);
            if (rconMonitorDto == null) return NotFound();

            var canViewRconMonitor = await _authorizationService.AuthorizeAsync(User, rconMonitorDto, AuthPolicies.ViewRconMonitor);

            if (!canViewRconMonitor.Succeeded)
                return Unauthorized();

            return View(rconMonitorDto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var rconMonitorDto = await _rconMonitorsRepository.GetRconMonitor(id);
            if (rconMonitorDto == null) return NotFound();

            var canEditRconMonitor = await _authorizationService.AuthorizeAsync(User, rconMonitorDto, AuthPolicies.EditRconMonitor);

            if (!canEditRconMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(rconMonitorDto.ServerId);

            return View(rconMonitorDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RconMonitorDto model)
        {
            var rconMonitorDto = await _rconMonitorsRepository.GetRconMonitor(model.RconMonitorId);
            if (rconMonitorDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(rconMonitorDto);
            }

            var canEditRconMonitor = await _authorizationService.AuthorizeAsync(User, rconMonitorDto, AuthPolicies.EditRconMonitor);

            if (!canEditRconMonitor.Succeeded)
                return Unauthorized();

            rconMonitorDto.MonitorMapRotation = model.MonitorMapRotation;
            rconMonitorDto.MonitorPlayers = model.MonitorPlayers;
            rconMonitorDto.MonitorPlayerIps = model.MonitorPlayerIps;

            await _rconMonitorsRepository.UpdateRconMonitor(rconMonitorDto);

            _logger.LogInformation(EventIds.Management, "User {User} has updated {RconMonitorId} against {ServerId}", User.Username(), rconMonitorDto.RconMonitorId, rconMonitorDto.ServerId);
            this.AddAlertSuccess($"The rcon monitor has been created for {rconMonitorDto.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var rconMonitorDto = await _rconMonitorsRepository.GetRconMonitor(id);
            if (rconMonitorDto == null) return NotFound();

            var canDeleteRconMonitor = await _authorizationService.AuthorizeAsync(User, rconMonitorDto, AuthPolicies.DeleteRconMonitor);

            if (!canDeleteRconMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(rconMonitorDto.ServerId);

            return View(rconMonitorDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var fileMonitorDto = await _rconMonitorsRepository.GetRconMonitor(id);
            if (fileMonitorDto == null) return NotFound();

            var canDeleteFileMonitor = await _authorizationService.AuthorizeAsync(User, fileMonitorDto, AuthPolicies.DeleteRconMonitor);

            if (!canDeleteFileMonitor.Succeeded)
                return Unauthorized();

            await _rconMonitorsRepository.DeleteRconMonitor(id);

            _logger.LogInformation(EventIds.Management, "User {User} has deleted {RconMonitorId} against {ServerId}", User.Username(), fileMonitorDto.RconMonitorId, fileMonitorDto.ServerId);
            this.AddAlertSuccess($"The rcon monitor has been deleted for {fileMonitorDto.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var filterModel = new GameServerFilterModel
            {
                Order = GameServerFilterModel.OrderBy.BannerServerListPosition
            }.ApplyAuthForRconMonitors(User);
            var gameServerDtos = await _gameServersRepository.GetGameServers(filterModel);
            ViewData["GameServers"] = new SelectList(gameServerDtos, "ServerId", "Title", selected);
        }
    }
}