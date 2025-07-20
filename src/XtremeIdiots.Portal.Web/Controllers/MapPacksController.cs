using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing map packs associated with Call of Duty game servers.
/// Provides functionality to create, manage and synchronize map packs for supported game types.
/// </summary>
/// <remarks>
/// This controller handles map pack operations for Call of Duty game servers including:
/// - Creating new map packs with titles, descriptions and game modes
/// - Synchronizing map packs to game servers via FTP integration
/// - Authorization validation based on game type permissions
/// - Integration with XtremeIdiots Portal repository and servers APIs
/// 
/// All operations require appropriate map management permissions and are validated against
/// the user's game type authorization levels.
/// </remarks>
[Authorize(Policy = AuthPolicies.ManageMaps)]
public class MapPacksController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;
 private readonly IServersApiClient serversApiClient;

 /// <summary>
 /// Initializes a new instance of the <see cref="MapPacksController"/> class.
 /// </summary>
 /// <param name="authorizationService">The authorization service for validating user permissions for map pack operations</param>
 /// <param name="repositoryApiClient">The repository API client for accessing map pack and game server data</param>
 /// <param name="serversApiClient">The servers API client for game server integration and FTP operations</param>
 /// <param name="telemetryClient">The Application Insights telemetry client for tracking map pack operations</param>
 /// <param name="logger">The logger instance for capturing map pack management events and errors</param>
 /// <param name="configuration">The configuration instance for accessing app settings and connection strings</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
 public MapPacksController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 IServersApiClient serversApiClient,
 TelemetryClient telemetryClient,
 ILogger<MapPacksController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
 }

 /// <summary>
 /// Displays the creation form for a new map pack associated with a specific Call of Duty game server.
 /// </summary>
 /// <param name="gameServerId">The unique identifier of the game server to create a map pack for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// create map pack view with pre-populated form data including game server context,
 /// or NotFound if the game server doesn't exist, or Unauthorized if user lacks permissions
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate map packs for the specified game type</exception>
 /// <exception cref="KeyNotFoundException">Thrown when the specified game server is not found in the repository</exception>
 /// <exception cref="InvalidOperationException">Thrown when the repository API An unexpected response format</exception>
 [HttpGet]
 public async Task<IActionResult> Create(Guid gameServerId, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
 gameServerId,
 AuthPolicies.CreateMapPack,
 nameof(Create),
 cancellationToken);

 if (actionResult != null) return actionResult;

 ViewData["GameServer"] = gameServerData;

 return View(new CreateMapPackViewModel
 {
 GameServerId = gameServerId,
 Title = string.Empty,
 Description = string.Empty,
 GameMode = string.Empty
 });
 }, nameof(Create));
 }

 /// <summary>
 /// Creates a new map pack for a Call of Duty game server based on the submitted form data.
 /// Validates the input, creates the map pack in the repository and optionally synchronizes to the game server.
 /// </summary>
 /// <param name="model">The create map pack view model containing the map pack details including title, description and game mode</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// A redirect to the map manager page on successful creation with success notification,
 /// or view with validation errors if creation fails or model validation fails
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate map packs for the specified game type</exception>
 /// <exception cref="KeyNotFoundException">Thrown when the specified game server is not found in the repository</exception>
 /// <exception cref="InvalidOperationException">Thrown when the repository API An unexpected response format or creation fails</exception>
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Create(CreateMapPackViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
 model.GameServerId,
 AuthPolicies.CreateMapPack,
 nameof(Create),
 cancellationToken);

 if (actionResult != null) return actionResult;

 var modelValidationResult = CheckModelState(model, m =>
 {
 ViewData["GameServer"] = gameServerData;
 });
 if (modelValidationResult != null) return modelValidationResult;

 var createMapPackDto = new CreateMapPackDto(model.GameServerId, model.Title, model.Description)
 {
 GameMode = model.GameMode,
 SyncToGameServer = model.SyncToGameServer
 };

 var createMapPackApiResponse = await repositoryApiClient.MapPacks.V1.CreateMapPack(createMapPackDto, cancellationToken);

 if (!createMapPackApiResponse.IsSuccess)
 {
 if (createMapPackApiResponse.Result?.Errors != null)
 {
 foreach (var error in createMapPackApiResponse.Result.Errors)
 {
 ModelState.AddModelError(error.Target ?? string.Empty, error.Message ?? "An error occurred");
 }
 }
 else
 {
 ModelState.AddModelError(string.Empty, "An error occurred while creating the map pack");
 }

 ViewData["GameServer"] = gameServerData;
 return View(model);
 }

 TrackSuccessTelemetry(nameof(Create), "MapPackCreated", new Dictionary<string, string>
 {
 { nameof(model.GameServerId), model.GameServerId.ToString() },
 { nameof(model.Title), model.Title },
 { "GameType", gameServerData!.GameType.ToString() }
 });

 this.AddAlertSuccess($"Map pack '{model.Title}' has been created successfully for {gameServerData.Title}.");

 return RedirectToAction("Manage", "MapManager", new { id = model.GameServerId });
 }, nameof(Create));
 }

 /// <summary>
 /// Helper method to retrieve game server data and validate user authorization for map pack operations.
 /// </summary>
 /// <param name="gameServerId">The unique identifier of the game server to authorize access for</param>
 /// <param name="policy">The authorization policy to validate against user permissions</param>
 /// <param name="action">The specific action being performed for telemetry and logging purposes</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// A tuple containing:
 /// - ActionResult: Non-null if authorization fails (Unauthorized) or game server not found (NotFound)
 /// - GameServerData: The game server information if authorization succeeds and server exists
 /// </returns>
 /// <exception cref="InvalidOperationException">Thrown when the repository API An unexpected response format</exception>
 private async Task<(IActionResult? ActionResult, GameServerDto? GameServerData)> GetAuthorizedGameServerAsync(
 Guid gameServerId,
 string policy,
 string action,
 CancellationToken cancellationToken = default)
 {
 var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(gameServerId, cancellationToken);

 if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Game server {GameServerId} not found when {Action} map pack", gameServerId, action);
 return (NotFound(), null);
 }

 var gameServerData = gameServerApiResponse.Result.Data;
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 gameServerData.GameType,
 policy,
 action,
 "MapPack",
 $"GameType:{gameServerData.GameType},GameServerId:{gameServerId}",
 gameServerData);

 return authResult is not null ? (authResult, null) : (null, gameServerData);
 }
}