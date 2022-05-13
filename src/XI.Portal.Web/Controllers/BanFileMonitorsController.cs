using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
    public class BanFileMonitorsController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly ILogger<BanFileMonitorsController> _logger;

        public BanFileMonitorsController(
            ILogger<BanFileMonitorsController> logger,
            IAuthorizationService authorizationService,
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryTokenProvider = repositoryTokenProvider;
            this.repositoryApiClient = repositoryApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitors = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(accessToken, gameTypes, banFileMonitorIds, null, 0, 0, "BannerServerListPosition");

            List<BanFileMonitorViewModel> models = new List<BanFileMonitorViewModel>();
            foreach (var banFileMonitor in banFileMonitors)
            {
                var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitor.ServerId);

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
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, model.ServerId);

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

            await repositoryApiClient.GameServers.CreateBanFileMonitorForGameServer(accessToken, model.ServerId, banFileMonitorDto);

            _logger.LogInformation("User {User} has created a new ban file monitor with Id {Id}", User.Username(), banFileMonitorDto.BanFileMonitorId);
            this.AddAlertSuccess($"The ban file monitor has been created for {gameServerDto.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(accessToken, id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitorDto.ServerId);

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
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(accessToken, id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitorDto.ServerId);

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
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(accessToken, model.BanFileMonitorId);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitorDto.ServerId);
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

            await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(accessToken, banFileMonitorDto);

            _logger.LogInformation("User {User} has updated {BanFileMonitorId} against {ServerId}", User.Username(), banFileMonitorDto.BanFileMonitorId, banFileMonitorDto.ServerId);
            this.AddAlertSuccess($"The ban file monitor has been created");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(accessToken, id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitorDto.ServerId);

            if (banFileMonitorDto == null) return NotFound();

            var canDeleteBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorDto.ServerId);

            var gameServerDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitorDto.ServerId);

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
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var banFileMonitorDto = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(accessToken, id);
            var serverDto = await repositoryApiClient.GameServers.GetGameServer(accessToken, banFileMonitorDto.ServerId);

            if (banFileMonitorDto == null) return NotFound();

            var canDeleteBanFileMonitor = await _authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(serverDto.GameType, serverDto.Id), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await repositoryApiClient.BanFileMonitors.DeleteBanFileMonitor(accessToken, id);

            _logger.LogInformation("User {User} has deleted {BanFileMonitorId} against {ServerId}", User.Username(), banFileMonitorDto.BanFileMonitorId, banFileMonitorDto.ServerId);
            this.AddAlertSuccess($"The ban file monitor has been deleted");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();

            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, serverIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServerDtos = await repositoryApiClient.GameServers.GetGameServers(accessToken, gameTypes, serverIds, null, 0, 0, "BannerServerListPosition");

            ViewData["GameServers"] = new SelectList(gameServerDtos, "Id", "Title", selected);
        }
    }
}