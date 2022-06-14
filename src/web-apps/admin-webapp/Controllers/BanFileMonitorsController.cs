using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.BanFileMonitors;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
    public class BanFileMonitorsController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;

        public BanFileMonitorsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient
            )
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition);

            if (!banFileMonitorsApiResponse.IsSuccess || banFileMonitorsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(banFileMonitorsApiResponse.Result.Entries);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await AddGameServersViewData();
            return View(new CreateBanFileMonitorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBanFileMonitorViewModel model)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.GetGameServer(model.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.GameServerId);
                return View(model);
            }

            var canCreateBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerApiResponse.Result.GameType, gameServerApiResponse.Result.GameServerId), AuthPolicies.CreateBanFileMonitor);

            if (!canCreateBanFileMonitor.Succeeded)
                return Unauthorized();

            var createBanFileMonitorDto = new CreateBanFileMonitorDto(model.GameServerId, model.FilePath, gameServerApiResponse.Result.GameType);
            await repositoryApiClient.BanFileMonitors.CreateBanFileMonitor(createBanFileMonitorDto);

            var eventTelemetry = new EventTelemetry("CreateBanFileMonitor").Enrich(User).Enrich(gameServerApiResponse.Result).Enrich(createBanFileMonitorDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The ban file monitor has been created for {gameServerApiResponse.Result.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.GameServer == null)
                return NotFound();

            var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.GameServer.GameType, banFileMonitorApiResponse.Result.GameServer.GameServerId), AuthPolicies.ViewBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            return View(banFileMonitorApiResponse.Result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.GameServer == null)
                return NotFound();

            var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.GameServer.GameType, banFileMonitorApiResponse.Result.GameServer.GameServerId), AuthPolicies.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorApiResponse.Result.GameServerId);

            var viewModel = new EditBanFileMonitorViewModel
            {
                BanFileMonitorId = banFileMonitorApiResponse.Result.BanFileMonitorId,
                FilePath = banFileMonitorApiResponse.Result.FilePath,
                RemoteFileSize = banFileMonitorApiResponse.Result.RemoteFileSize,
                LastSync = banFileMonitorApiResponse.Result.LastSync,
                GameServerId = banFileMonitorApiResponse.Result.GameServerId,
                GameServer = banFileMonitorApiResponse.Result.GameServer
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditBanFileMonitorViewModel model)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(model.BanFileMonitorId);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.GameServer == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.GameServerId);
                model.GameServer = banFileMonitorApiResponse.Result.GameServer;
                return View(model);
            }

            var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.GameServer.GameType, banFileMonitorApiResponse.Result.GameServer.GameServerId), AuthPolicies.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorApiResponse.Result.BanFileMonitorId, model.FilePath);
            await repositoryApiClient.BanFileMonitors.UpdateBanFileMonitor(editBanFileMonitorDto);

            var eventTelemetry = new EventTelemetry("EditBanFileMonitor").Enrich(User).Enrich(banFileMonitorApiResponse.Result.GameServer).Enrich(editBanFileMonitorDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The ban file monitor has been updated for {banFileMonitorApiResponse.Result.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.GameServer == null)
                return NotFound();

            var canDeleteBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.GameServer.GameType, banFileMonitorApiResponse.Result.GameServer.GameServerId), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorApiResponse.Result.GameServerId);

            return View(banFileMonitorApiResponse.Result);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.GameServer == null)
                return NotFound();

            var canDeleteBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.GameServer.GameType, banFileMonitorApiResponse.Result.GameServer.GameServerId), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await repositoryApiClient.BanFileMonitors.DeleteBanFileMonitor(id);

            var eventTelemetry = new EventTelemetry("DeleteBanFileMonitor").Enrich(User).Enrich(banFileMonitorApiResponse.Result.GameServer).Enrich(banFileMonitorApiResponse.Result);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The ban file monitor has been deleted for {banFileMonitorApiResponse.Result.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var requiredClaims = new[] { XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, PortalClaimTypes.BanFileMonitor };
            var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            if (gameServersApiResponse.Result != null)
                ViewData["GameServers"] = new SelectList(gameServersApiResponse.Result.Entries, nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title), selected);
        }
    }
}