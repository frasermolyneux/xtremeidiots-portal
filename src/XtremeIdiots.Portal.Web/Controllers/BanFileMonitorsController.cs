using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.BanFileMonitors;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing ban file monitors that track and synchronize ban files from game servers.
/// Provides functionality to create, view, edit and delete ban file monitors with game-specific authorization.
/// </summary>
[Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
public class BanFileMonitorsController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<BanFileMonitorsController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
 private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

 /// <summary>
 /// Displays the list of ban file monitors accessible to the current user.
 /// Filters results based on user's game-specific permissions and claims.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// The ban file monitors index view with accessible monitors, 
 /// or Redirects to error page if API call fails
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionaccess ban file monitors</exception>
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
 }, nameof(Index));
 }

 /// <summary>
 /// Displays the create ban file monitor form with available game servers.
 /// Populates dropdown with game servers accessible to the current user.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// The create ban file monitor view with populated game servers dropdown,
 /// or error response if view data cannot be loaded
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate ban file monitors</exception>
 [HttpGet]
 public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 await AddGameServersViewData(cancellationToken: cancellationToken);
 return View(new CreateBanFileMonitorViewModel { FilePath = string.Empty });
 }, nameof(Create));
 }

 /// <summary>
 /// Creates a new ban file monitor for a specified game server.
 /// Validates user authorization for the target game server and creates the monitor
 /// with the specified file path.
 /// </summary>
 /// <param name="model">The create ban file monitor view model containing form data</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// Redirects to index with success message on successful creation,
 /// returns view with validation errors if model is invalid,
 /// returns NotFound if game server doesn't exist
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate ban file monitors</exception>
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
 m.GameServer = gameServerData;
 });
 if (modelValidationResult is not null) return modelValidationResult;

 var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 authorizationResource,
 AuthPolicies.CreateBanFileMonitor,
 nameof(Create),
 "BanFileMonitor",
 $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}",
 gameServerData);

 if (authResult is not null) return authResult;

 var createBanFileMonitorDto = new CreateBanFileMonitorDto(model.GameServerId, model.FilePath, gameServerData.GameType);
 await repositoryApiClient.BanFileMonitors.V1.CreateBanFileMonitor(createBanFileMonitorDto, cancellationToken);

 TrackSuccessTelemetry("BanFileMonitorCreated", nameof(Create), new Dictionary<string, string>
 {
 { nameof(model.GameServerId), model.GameServerId.ToString() },
 { nameof(model.FilePath), model.FilePath },
 { nameof(GameType), gameServerData.GameType.ToString() }
 });

 this.AddAlertSuccess($"The ban file monitor has been created for {gameServerData.Title}");

 return RedirectToAction(nameof(Index));
 }, nameof(Create));
 }

 /// <summary>
 /// Displays details for a specific ban file monitor including sync status and configuration.
 /// Validates user authorization before showing detailed information.
 /// </summary>
 /// <param name="id">The ban file monitor ID</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// The ban file monitor details view with full monitor information,
 /// returns NotFound if monitor doesn't exist,
 /// returns authorization error if user lacks access
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionview ban file monitor</exception>
 /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
 [HttpGet]
 public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.ViewBanFileMonitor, nameof(Details), cancellationToken);

 if (actionResult is not null) return actionResult;

 return View(banFileMonitorData);
 }, nameof(Details));
 }

 /// <summary>
 /// Displays the edit form for a ban file monitor with pre-populated data.
 /// Loads current monitor configuration and populates available game servers.
 /// </summary>
 /// <param name="id">The ban file monitor ID</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// The edit ban file monitor view with current data and game servers dropdown,
 /// returns NotFound if monitor doesn't exist,
 /// returns authorization error if user lacks access
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionedit ban file monitor</exception>
 /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
 [HttpGet]
 public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.EditBanFileMonitor, nameof(Edit), cancellationToken);

 if (actionResult is not null) return actionResult;

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
 }, nameof(Edit));
 }

 /// <summary>
 /// Updates an existing ban file monitor with new configuration.
 /// Validates user authorization and model state before applying changes.
 /// </summary>
 /// <param name="model">The edit ban file monitor view model containing updated form data</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// Redirects to index with success message on successful update,
 /// returns view with validation errors if model is invalid,
 /// returns authorization error if user lacks access
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionedit ban file monitor</exception>
 /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(EditBanFileMonitorViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 model.BanFileMonitorId, AuthPolicies.EditBanFileMonitor, nameof(Edit), cancellationToken);

 if (actionResult is not null) return actionResult;

 var modelValidationResult = await CheckModelStateAsync(model, async m =>
 {
 await AddGameServersViewData(model.GameServerId, cancellationToken);
 model.GameServer = banFileMonitorData!.GameServer;
 });
 if (modelValidationResult is not null) return modelValidationResult;

 var editBanFileMonitorDto = new EditBanFileMonitorDto(banFileMonitorData!.BanFileMonitorId, model.FilePath);
 await repositoryApiClient.BanFileMonitors.V1.UpdateBanFileMonitor(editBanFileMonitorDto, cancellationToken);

 TrackSuccessTelemetry("BanFileMonitorUpdated", nameof(Edit), new Dictionary<string, string>
 {
 { nameof(model.BanFileMonitorId), model.BanFileMonitorId.ToString() },
 { nameof(model.FilePath), model.FilePath },
 { nameof(GameType), banFileMonitorData.GameServer.GameType.ToString() }
 });

 this.AddAlertSuccess($"The ban file monitor has been updated for {banFileMonitorData.GameServer.Title}");

 return RedirectToAction(nameof(Index));
 }, nameof(Edit));
 }

 /// <summary>
 /// Displays the delete confirmation for a ban file monitor.
 /// Shows monitor details to confirm the deletion operation.
 /// </summary>
 /// <param name="id">The ban file monitor ID</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// The delete ban file monitor confirmation view with monitor details,
 /// returns NotFound if monitor doesn't exist,
 /// returns authorization error if user lacks access
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete ban file monitor</exception>
 /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
 [HttpGet]
 public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.DeleteBanFileMonitor, nameof(Delete), cancellationToken);

 if (actionResult is not null) return actionResult;

 await AddGameServersViewData(banFileMonitorData!.GameServerId, cancellationToken);

 return View(banFileMonitorData);
 }, nameof(Delete));
 }

 /// <summary>
 /// Deletes a ban file monitor after confirmation.
 /// Performs final authorization check and removes the monitor from the system.
 /// </summary>
 /// <param name="id">The ban file monitor ID</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>
 /// Redirects to index with success message on successful deletion,
 /// Redirects to index with error alert on failure,
 /// returns authorization error if user lacks access
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete ban file monitor</exception>
 /// <exception cref="KeyNotFoundException">Thrown when ban file monitor is not found</exception>
 [HttpPost]
 [ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, banFileMonitorData) = await GetAuthorizedBanFileMonitorAsync(
 id, AuthPolicies.DeleteBanFileMonitor, nameof(DeleteConfirmed), cancellationToken);

 if (actionResult is not null) return actionResult;

 await repositoryApiClient.BanFileMonitors.V1.DeleteBanFileMonitor(id, cancellationToken);

 TrackSuccessTelemetry("BanFileMonitorDeleted", nameof(DeleteConfirmed), new Dictionary<string, string>
 {
 { nameof(id), id.ToString() },
 { nameof(GameType), banFileMonitorData!.GameServer.GameType.ToString() },
 { "GameServerId", banFileMonitorData.GameServer.GameServerId.ToString() }
 });

 this.AddAlertSuccess($"The ban file monitor has been deleted for {banFileMonitorData.GameServer.Title}");

 return RedirectToAction(nameof(Index));
 }, nameof(DeleteConfirmed));
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

 if (authResult is not null)
 {
 return (authResult, null);
 }

 return (null, banFileMonitorData);
 }
}
