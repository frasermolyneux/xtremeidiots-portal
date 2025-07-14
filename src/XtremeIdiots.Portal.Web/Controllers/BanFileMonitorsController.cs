using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class BanFileMonitorsController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<BanFileMonitorsController> logger;

        public BanFileMonitorsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<BanFileMonitorsController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view ban file monitors index",
                    User.XtremeIdiotsId());

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
                var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

                var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition, cancellationToken);

                if (!banFileMonitorsApiResponse.IsSuccess || banFileMonitorsApiResponse.Result?.Data?.Items is null)
                {
                    logger.LogError("Failed to retrieve ban file monitors for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("User {UserId} successfully loaded {Count} ban file monitors",
                    User.XtremeIdiotsId(), banFileMonitorsApiResponse.Result.Data.Items.Count());

                return View(banFileMonitorsApiResponse.Result.Data.Items);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading ban file monitors index for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view create ban file monitor form",
                    User.XtremeIdiotsId());

                await AddGameServersViewData(cancellationToken: cancellationToken);

                logger.LogInformation("User {UserId} successfully loaded create ban file monitor form",
                    User.XtremeIdiotsId());

                return View(new CreateBanFileMonitorViewModel());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading create ban file monitor form for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to create ban file monitor for game server {GameServerId}",
                    User.XtremeIdiotsId(), model.GameServerId);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId, cancellationToken);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    logger.LogWarning("Game server {GameServerId} not found when creating ban file monitor", model.GameServerId);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for creating ban file monitor for game server {GameServerId}", model.GameServerId);
                    await AddGameServersViewData(model.GameServerId, cancellationToken);
                    return View(model);
                }

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var canCreateBanFileMonitor = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.CreateBanFileMonitor);

                if (!canCreateBanFileMonitor.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create ban file monitor for game server {GameServerId}",
                        User.XtremeIdiotsId(), model.GameServerId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "BanFileMonitors");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileMonitor");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var createBanFileMonitorDto = new CreateBanFileMonitorDto(model.GameServerId, model.FilePath, gameServerData.GameType);
                await repositoryApiClient.BanFileMonitors.V1.CreateBanFileMonitor(createBanFileMonitorDto, cancellationToken);

                var eventTelemetry = new EventTelemetry("BanFileMonitorCreated");
                eventTelemetry
                    .Enrich(User)
                    .Enrich(gameServerData)
                    .Enrich(createBanFileMonitorDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully created ban file monitor for game server {GameServerId}",
                    User.XtremeIdiotsId(), model.GameServerId);

                this.AddAlertSuccess($"The ban file monitor has been created for {gameServerData.Title}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating ban file monitor for game server {GameServerId}", model.GameServerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", model.GameServerId.ToString());
                errorTelemetry.Properties.TryAdd("FilePath", model.FilePath);
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while creating the ban file monitor. Please try again.");

                await AddGameServersViewData(model.GameServerId, cancellationToken);
                return View(model);
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view ban file monitor details {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

                if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
                {
                    logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when viewing details", id);
                    return NotFound();
                }

                var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
                var gameServerData = banFileMonitorData.GameServer;

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var canViewBanFileMonitor = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.ViewBanFileMonitor);

                if (!canViewBanFileMonitor.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view ban file monitor {BanFileMonitorId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(banFileMonitorData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "BanFileMonitors");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "View");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileMonitor");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully viewed ban file monitor details {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                return View(banFileMonitorData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading ban file monitor details {BanFileMonitorId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("BanFileMonitorId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to edit ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

                if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
                {
                    logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when loading edit form", id);
                    return NotFound();
                }

                var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
                var gameServerData = banFileMonitorData.GameServer;

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.EditBanFileMonitor);

                if (!canEditBanFileMonitor.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit ban file monitor {BanFileMonitorId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(banFileMonitorData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "BanFileMonitors");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileMonitor");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await AddGameServersViewData(banFileMonitorData.GameServerId, cancellationToken);

                var viewModel = new EditBanFileMonitorViewModel
                {
                    BanFileMonitorId = banFileMonitorData.BanFileMonitorId,
                    FilePath = banFileMonitorData.FilePath,
                    RemoteFileSize = banFileMonitorData.RemoteFileSize,
                    LastSync = banFileMonitorData.LastSync,
                    GameServerId = banFileMonitorData.GameServerId,
                    GameServer = gameServerData
                };

                logger.LogInformation("User {UserId} successfully loaded edit form for ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading edit form for ban file monitor {BanFileMonitorId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("BanFileMonitorId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to update ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), model.BanFileMonitorId);

                var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(model.BanFileMonitorId, cancellationToken);

                if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
                {
                    logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when updating", model.BanFileMonitorId);
                    return NotFound();
                }

                var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
                var gameServerData = banFileMonitorData.GameServer;

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for updating ban file monitor {BanFileMonitorId}", model.BanFileMonitorId);
                    await AddGameServersViewData(model.GameServerId, cancellationToken);
                    model.GameServer = gameServerData;
                    return View(model);
                }

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var canEditBanFileMonitor = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.EditBanFileMonitor);

                if (!canEditBanFileMonitor.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to update ban file monitor {BanFileMonitorId}",
                        User.XtremeIdiotsId(), model.BanFileMonitorId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(banFileMonitorData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "BanFileMonitors");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileMonitor");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorData.BanFileMonitorId, model.FilePath);
                await repositoryApiClient.BanFileMonitors.V1.UpdateBanFileMonitor(editBanFileMonitorDto, cancellationToken);

                var eventTelemetry = new EventTelemetry("BanFileMonitorUpdated");
                eventTelemetry
                    .Enrich(User)
                    .Enrich(gameServerData)
                    .Enrich(editBanFileMonitorDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully updated ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), model.BanFileMonitorId);

                this.AddAlertSuccess($"The ban file monitor has been updated for {gameServerData.Title}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating ban file monitor {BanFileMonitorId}", model.BanFileMonitorId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry
                    .Enrich(User)
                    .Enrich(new EditBanFileMonitorDto(model.BanFileMonitorId, model.FilePath));
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while updating the ban file monitor. Please try again.");

                await AddGameServersViewData(model.GameServerId, cancellationToken);
                return View(model);
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view delete confirmation for ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

                if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
                {
                    logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when loading delete confirmation", id);
                    return NotFound();
                }

                var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
                var gameServerData = banFileMonitorData.GameServer;

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var canDeleteBanFileMonitor = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.DeleteBanFileMonitor);

                if (!canDeleteBanFileMonitor.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete ban file monitor {BanFileMonitorId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(banFileMonitorData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "BanFileMonitors");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileMonitor");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await AddGameServersViewData(banFileMonitorData.GameServerId, cancellationToken);

                logger.LogInformation("User {UserId} successfully loaded delete confirmation for ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                return View(banFileMonitorData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading delete confirmation for ban file monitor {BanFileMonitorId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("BanFileMonitorId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to delete ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                var banFileMonitorApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

                if (banFileMonitorApiResponse.IsNotFound || banFileMonitorApiResponse.Result?.Data?.GameServer is null)
                {
                    logger.LogWarning("Ban file monitor {BanFileMonitorId} not found when deleting", id);
                    return NotFound();
                }

                var banFileMonitorData = banFileMonitorApiResponse.Result.Data;
                var gameServerData = banFileMonitorData.GameServer;

                var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
                var canDeleteBanFileMonitor = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.DeleteBanFileMonitor);

                if (!canDeleteBanFileMonitor.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete ban file monitor {BanFileMonitorId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(banFileMonitorData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "BanFileMonitors");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileMonitor");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await repositoryApiClient.BanFileMonitors.V1.DeleteBanFileMonitor(id, cancellationToken);

                var eventTelemetry = new EventTelemetry("BanFileMonitorDeleted");
                eventTelemetry
                    .Enrich(User)
                    .Enrich(gameServerData)
                    .Enrich(banFileMonitorData);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully deleted ban file monitor {BanFileMonitorId}",
                    User.XtremeIdiotsId(), id);

                this.AddAlertSuccess($"The ban file monitor has been deleted for {gameServerData.Title}");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting ban file monitor {BanFileMonitorId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("BanFileMonitorId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while deleting the ban file monitor. Please try again.");
                return RedirectToAction(nameof(Index));
            }
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
                    logger.LogWarning("Failed to load game servers for user {UserId}", User.XtremeIdiotsId());
                    ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading game servers view data for user {UserId}", User.XtremeIdiotsId());
                ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), nameof(GameServerDto.GameServerId), nameof(GameServerDto.Title));
            }
        }
    }
}