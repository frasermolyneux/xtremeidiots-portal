using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Web.Auth.Constants;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
    public class BanFileMonitorsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<BanFileMonitorsController> _logger;

        public BanFileMonitorsController(
            ILogger<BanFileMonitorsController> logger,
            IAuthorizationService authorizationService,

            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitors = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 0, "BannerServerListPosition");

            List<BanFileMonitorViewModel> models = new List<BanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitors)
            {
                var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitor.ServerId);

                models.Add(new BanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                    FilePath = banFileMonitor.FilePath,
                    RemoteFileSize = banFileMonitor.RemoteFileSize,
                    LastSync = banFileMonitor.LastSync,
                    ServerId = banFileMonitor.ServerId,
                    GameServer = gameServerDto
                });
            }

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await AddGameServersViewData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BanFileMonitorViewModel model)
        {
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(model.ServerId);

            if (gameServerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(model);
            }

            var banFileMonitorDto = new BanFileMonitorDto
            {
                ServerId = model.ServerId
            };

            var canCreateBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerDto.GameType, gameServerDto.Id), AuthPolicies.CreateBanFileMonitor);

            if (!canCreateBanFileMonitor.Succeeded)
                return Unauthorized();

            banFileMonitorDto.FilePath = model.FilePath;

            await repositoryApiClient.GameServers.CreateBanFileMonitorForGameServer(model.ServerId, banFileMonitorDto);

            _logger.LogInformation("User {User} has created a new ban file monitor with Id {Id}", User.Username(), banFileMonitorDto.BanFileMonitorId);
            this.AddAlertSuccess($"The ban file monitor has been created for {gameServerDto.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);

            if (banFileMonitorDto == null) return NotFound();

            var canEditBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.ViewBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            var viewModel = new BanFileMonitorViewModel
            {
                BanFileMonitorId = banFileMonitorDto.BanFileMonitorId,
                FilePath = banFileMonitorDto.FilePath,
                RemoteFileSize = banFileMonitorDto.RemoteFileSize,
                LastSync = banFileMonitorDto.LastSync,
                ServerId = banFileMonitorDto.ServerId,
                GameServer = serverDto
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {

            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);

            if (banFileMonitorDto == null) return NotFound();

            var canEditBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorDto.ServerId);

            var viewModel = new BanFileMonitorViewModel
            {
                BanFileMonitorId = banFileMonitorDto.BanFileMonitorId,
                FilePath = banFileMonitorDto.FilePath,
                RemoteFileSize = banFileMonitorDto.RemoteFileSize,
                LastSync = banFileMonitorDto.LastSync,
                ServerId = banFileMonitorDto.ServerId,
                GameServer = serverDto
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BanFileMonitorViewModel model)
        {
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(model.BanFileMonitorId);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);
            if (banFileMonitorDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.ServerId);
                return View(banFileMonitorDto);
            }

            var canEditBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            banFileMonitorDto.FilePath = model.FilePath;

            await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(banFileMonitorDto);

            _logger.LogInformation("User {User} has updated {BanFileMonitorId} against {ServerId}", User.Username(), banFileMonitorDto.BanFileMonitorId, banFileMonitorDto.ServerId);
            this.AddAlertSuccess($"The ban file monitor has been created");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);

            if (banFileMonitorDto == null) return NotFound();

            var canDeleteBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorDto.ServerId);

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);

            var viewModel = new BanFileMonitorViewModel
            {
                BanFileMonitorId = banFileMonitorDto.BanFileMonitorId,
                FilePath = banFileMonitorDto.FilePath,
                RemoteFileSize = banFileMonitorDto.RemoteFileSize,
                LastSync = banFileMonitorDto.LastSync,
                ServerId = banFileMonitorDto.ServerId,
                GameServer = gameServerDto
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(banFileMonitorDto.ServerId);

            if (banFileMonitorDto == null) return NotFound();

            var canDeleteBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await repositoryApiClient.BanFileMonitors.DeleteBanFileMonitor(id);

            _logger.LogInformation("User {User} has deleted {BanFileMonitorId} against {ServerId}", User.Username(), banFileMonitorDto.BanFileMonitorId, banFileMonitorDto.ServerId);
            this.AddAlertSuccess($"The ban file monitor has been deleted");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(gameTypes, serverIds, null, 0, 0, "BannerServerListPosition");

            ViewData["GameServers"] = new SelectList(gameServerDtos, "Id", "Title", selected);
        }
    }
}