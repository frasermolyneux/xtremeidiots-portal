using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.BanFileMonitors;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
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
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition);

            if (!banFileMonitorsApiResponse.IsSuccess || banFileMonitorsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            return View(banFileMonitorsApiResponse.Result.Data.Items);
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
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.GameServerId);
                return View(model);
            }

            var canCreateBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(gameServerApiResponse.Result.Data.GameType, gameServerApiResponse.Result.Data.GameServerId), AuthPolicies.CreateBanFileMonitor);

            if (!canCreateBanFileMonitor.Succeeded)
                return Unauthorized();

            var createBanFileMonitorDto = new CreateBanFileMonitorDto(model.GameServerId, model.FilePath, gameServerApiResponse.Result.Data.GameType);
            await repositoryApiClient.BanFileMonitors.V1.CreateBanFileMonitor(createBanFileMonitorDto);

            var eventTelemetry = new EventTelemetry("CreateBanFileMonitor").Enrich(User).Enrich(gameServerApiResponse.Result.Data).Enrich(createBanFileMonitorDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The ban file monitor has been created for {gameServerApiResponse.Result.Data.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.Data.GameServer == null)
                return NotFound();

            var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.Data.GameServer.GameType, banFileMonitorApiResponse.Result.Data.GameServer.GameServerId), AuthPolicies.ViewBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            return View(banFileMonitorApiResponse.Result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.Data.GameServer == null)
                return NotFound();

            var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.Data.GameServer.GameType, banFileMonitorApiResponse.Result.Data.GameServer.GameServerId), AuthPolicies.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorApiResponse.Result.Data.GameServerId);

            var viewModel = new EditBanFileMonitorViewModel
            {
                BanFileMonitorId = banFileMonitorApiResponse.Result.Data.BanFileMonitorId,
                FilePath = banFileMonitorApiResponse.Result.Data.FilePath,
                RemoteFileSize = banFileMonitorApiResponse.Result.Data.RemoteFileSize,
                LastSync = banFileMonitorApiResponse.Result.Data.LastSync,
                GameServerId = banFileMonitorApiResponse.Result.Data.GameServerId,
                GameServer = banFileMonitorApiResponse.Result.Data.GameServer
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditBanFileMonitorViewModel model)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(model.BanFileMonitorId);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.Data.GameServer == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await AddGameServersViewData(model.GameServerId);
                model.GameServer = banFileMonitorApiResponse.Result.Data.GameServer;
                return View(model);
            }

            var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.Data.GameServer.GameType, banFileMonitorApiResponse.Result.Data.GameServer.GameServerId), AuthPolicies.EditBanFileMonitor);

            if (!canEditBanFileMonitor.Succeeded)
                return Unauthorized();

            var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorApiResponse.Result.Data.BanFileMonitorId, model.FilePath);
            await repositoryApiClient.BanFileMonitors.V1.UpdateBanFileMonitor(editBanFileMonitorDto);

            var eventTelemetry = new EventTelemetry("EditBanFileMonitor").Enrich(User).Enrich(banFileMonitorApiResponse.Result.Data.GameServer).Enrich(editBanFileMonitorDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The ban file monitor has been updated for {banFileMonitorApiResponse.Result.Data.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.Data.GameServer == null)
                return NotFound();

            var canDeleteBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.Data.GameServer.GameType, banFileMonitorApiResponse.Result.Data.GameServer.GameServerId), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await AddGameServersViewData(banFileMonitorApiResponse.Result.Data.GameServerId);

            return View(banFileMonitorApiResponse.Result.Data);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result == null || banFileMonitorApiResponse.Result.Data.GameServer == null)
                return NotFound();

            var canDeleteBanFileMonitor = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, Guid>(banFileMonitorApiResponse.Result.Data.GameServer.GameType, banFileMonitorApiResponse.Result.Data.GameServer.GameServerId), AuthPolicies.DeleteBanFileMonitor);

            if (!canDeleteBanFileMonitor.Succeeded)
                return Unauthorized();

            await repositoryApiClient.BanFileMonitors.V1.DeleteBanFileMonitor(id);

            var eventTelemetry = new EventTelemetry("DeleteBanFileMonitor").Enrich(User).Enrich(banFileMonitorApiResponse.Result.Data.GameServer).Enrich(banFileMonitorApiResponse.Result.Data);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The ban file monitor has been deleted for {banFileMonitorApiResponse.Result.Data.GameServer.Title}");

            return RedirectToAction(nameof(Index));
        }

        private async Task AddGameServersViewData(Guid? selected = null)
        {
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
            var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

            if (gameServersApiResponse.Result != null)
                ViewData["GameServers"] = new SelectList(gameServersApiResponse.Result.Data.Items, nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title), selected);
        }
    }
}