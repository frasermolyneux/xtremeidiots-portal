using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.BanFileMonitors;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing ban file monitors
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
    public class BanFileMonitorsController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public BanFileMonitorsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<BanFileMonitorsController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Displays the list of ban file monitors accessible to the current user
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The ban file monitors index view, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to access ban file monitors</exception>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
                var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

                var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition, cancellationToken);

                if (!banFileMonitorsApiResponse.IsSuccess || banFileMonitorsApiResponse.Result?.Data?.Items is null)
                {
                    Logger.LogError("Failed to retrieve ban file monitors for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                return View(banFileMonitorsApiResponse.Result.Data.Items);
            }, "LoadBanFileMonitorsIndex");
        }

        /// <summary>
        /// Displays the create ban file monitor form
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The create ban file monitor view, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create ban file monitors</exception>
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                await AddGameServersViewData(cancellationToken: cancellationToken);
                return View(new CreateBanFileMonitorViewModel());
            }, "LoadCreateBanFileMonitorForm");
        }

        /// <summary>
        /// Creates a new ban file monitor for a specified game server
        /// </summary>
        /// <param name="model">The create ban file monitor view model containing form data</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to index on success, returns view with validation errors on failure</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create ban file monitors</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBanFileMonitorViewModel model, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId, cancellationToken);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Game server {GameServerId} not found when creating ban file monitor", model.GameServerId);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                var modelValidationResult = await CheckModelStateAsync(model, async m =>
                {
                    await AddGameServersViewData(model.GameServerId, cancellationToken);
                });
                if (modelValidationResult != null) return modelValidationResult;

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    authorizationResource,
                    AuthPolicies.CreateBanFileMonitor,
                    "Create",
                    "BanFileMonitor",
                    $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}",
                    gameServerData);

                if (authResult != null) return authResult;

                var createBanFileMonitorDto = new CreateBanFileMonitorDto(model.GameServerId, model.FilePath, gameServerData.GameType);
                await repositoryApiClient.BanFileMonitors.V1.CreateBanFileMonitor(createBanFileMonitorDto, cancellationToken);

                TrackSuccessTelemetry("BanFileMonitorCreated", "CreateBanFileMonitor", new Dictionary<string, string>
                {
                    { "GameServerId", model.GameServerId.ToString() },
                    { "FilePath", model.FilePath },
                    { "GameType", gameServerData.GameType.ToString() }
                });

                this.AddAlertSuccess($"The ban file monitor has been created for {gameServerData.Title}");

                return RedirectToAction(nameof(Index));
            }, "CreateBanFileMonitor");
        }

        /// <summary>
        /// Displays details for a specific ban file monitor
        /// </summary>
        /// <param name="id">The ban file monitor ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The ban file monitor details view, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to view ban file monitor</exception>
        /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
                    id, AuthPolicies.ViewBanFileMonitor, "viewing details", cancellationToken);

                if (actionResult != null) return actionResult;

                return View(banFileMonitorData);
            }, "ViewBanFileMonitorDetails");
        }

        /// <summary>
        /// Displays the edit form for a ban file monitor
        /// </summary>
        /// <param name="id">The ban file monitor ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The edit ban file monitor view, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to edit ban file monitor</exception>
        /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
                    id, AuthPolicies.EditBanFileMonitor, "loading edit form", cancellationToken);

                if (actionResult != null) return actionResult;

                await AddGameServersViewData(banFileMonitorData!.GameServerId, cancellationToken);

                var viewModel = new EditBanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitorData.BanFileMonitorId,
                    FilePath = banFileMonitorData.FilePath,
                    RemoteFileSize = banFileMonitorData.RemoteFileSize,
                    LastSync = banFileMonitorData.LastSync,
                    GameServerId = banFileMonitorData.GameServerId,
                    GameServer = banFileMonitorData.GameServer
                };

                return View(viewModel);
            }, "LoadEditBanFileMonitorForm");
        }

        /// <summary>
        /// Updates an existing ban file monitor
        /// </summary>
        /// <param name="model">The edit ban file monitor view model containing updated form data</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to index on success, returns view with validation errors on failure</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to edit ban file monitor</exception>
        /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditBanFileMonitorViewModel model, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
                    model.BanFileMonitorId, AuthPolicies.EditBanFileMonitor, "updating", cancellationToken);

                if (actionResult != null) return actionResult;

                var modelValidationResult = await CheckModelStateAsync(model, async m =>
                {
                    await AddGameServersViewData(model.GameServerId, cancellationToken);
                    model.GameServer = banFileMonitorData!.GameServer;
                });
                if (modelValidationResult != null) return modelValidationResult;

                var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorData!.BanFileMonitorId, model.FilePath);
                await repositoryApiClient.BanFileMonitors.V1.UpdateBanFileMonitor(editBanFileMonitorDto, cancellationToken);

                TrackSuccessTelemetry("BanFileMonitorUpdated", "UpdateBanFileMonitor", new Dictionary<string, string>
                {
                    { "BanFileMonitorId", model.BanFileMonitorId.ToString() },
                    { "FilePath", model.FilePath },
                    { "GameType", banFileMonitorData.GameServer.GameType.ToString() }
                });

                this.AddAlertSuccess($"The ban file monitor has been updated for {banFileMonitorData.GameServer.Title}");

                return RedirectToAction(nameof(Index));
            }, "UpdateBanFileMonitor");
        }

        /// <summary>
        /// Displays the delete confirmation for a ban file monitor
        /// </summary>
        /// <param name="id">The ban file monitor ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The delete ban file monitor confirmation view, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete ban file monitor</exception>
        /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
                    id, AuthPolicies.DeleteBanFileMonitor, "loading delete confirmation", cancellationToken);

                if (actionResult != null) return actionResult;

                await AddGameServersViewData(banFileMonitorData!.GameServerId, cancellationToken);

                return View(banFileMonitorData);
            }, "LoadDeleteBanFileMonitorConfirmation");
        }

        /// <summary>
        /// Deletes a ban file monitor after confirmation
        /// </summary>
        /// <param name="id">The ban file monitor ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to index on success, redirects to index with error alert on failure</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete ban file monitor</exception>
        /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
                    id, AuthPolicies.DeleteBanFileMonitor, "deleting", cancellationToken);

                if (actionResult != null) return actionResult;

                await repositoryApiClient.BanFileMonitors.V1.DeleteBanFileMonitor(id, cancellationToken);

                TrackSuccessTelemetry("BanFileMonitorDeleted", "DeleteBanFileMonitor", new Dictionary<string, string>
                {
                    { "BanFileMonitorId", id.ToString() },
                    { "GameType", banFileMonitorData!.GameServer.GameType.ToString() },
                    { "GameServerId", banFileMonitorData.GameServer.GameServerId.ToString() }
                });

                this.AddAlertSuccess($"The ban file monitor has been deleted for {banFileMonitorData.GameServer.Title}");

                return RedirectToAction(nameof(Index));
            }, "DeleteBanFileMonitor");
        }

        /// <summary>
        /// Adds game servers view data for dropdown selection
        /// </summary>
        /// <param name="selected">The selected game server ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        private async Task AddGameServersViewData(Guid? selected = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition, cancellationToken);

                if (gameServersApiResponse.Result?.Data?.Items is not null)
                {
                    ViewData["GameServers"] = new SelectList(gameServersApiResponse.Result.Data.Items, nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title), selected);
                }
                else
                {
                    Logger.LogWarning("Failed to load game servers for user {UserId}", User.XtremeIdiotsId());
                    ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading game servers view data for user {UserId}", User.XtremeIdiotsId());
                ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title));
            }
        }

        /// <summary>
        /// Retrieves and authorizes access to a ban file monitor
        /// </summary>
        /// <param name="id">The ban file monitor ID</param>
        /// <param name="policy">The authorization policy to check</param>
        /// <param name="action">The action being performed for logging</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Tuple containing the action result if unauthorized/not found and the ban file monitor data if successful</returns>
        private async Task<(IActionResult? ActionResult, BanFileMonitorDto? BanFileMonitor)> GetAuthorizedBanFileMonitorAsync(
            Guid id,
            string policy,
            string action,
            CancellationToken cancellationToken = default)
        {
            var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

            if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
            {
                Logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when {Action}", id, action);
                return (NotFound(), null);
            }

            var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
            var gameServerData = banFileMonitorData.GameServer;

            var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                policy,
                action,
                "BanFileMonitor",
                $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}",
                banFileMonitorData);

            if (authResult != null)
            {
                return (authResult, null);
            }

            return (null, banFileMonitorData);
        }
    }
}
