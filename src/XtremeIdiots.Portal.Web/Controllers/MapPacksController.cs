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
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing map packs associated with game servers
    /// </summary>
    [Authorize(Policy = AuthPolicies.ManageMaps)]
    public class MapPacksController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<MapPacksController> logger;

        public MapPacksController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            TelemetryClient telemetryClient,
            ILogger<MapPacksController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the creation form for a new map pack associated with a specific game server
        /// </summary>
        /// <param name="gameServerId">The game server ID to create a map pack for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The create map pack view with pre-populated form data, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create map packs</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Create(Guid gameServerId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to create map pack for game server {GameServerId}",
                    User.XtremeIdiotsId(), gameServerId);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(gameServerId);

                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Game server {GameServerId} not found when creating map pack", gameServerId);
                    return NotFound();
                }

                if (gameServerApiResponse.Result?.Data is null)
                {
                    logger.LogWarning("Game server data is null for {GameServerId}", gameServerId);
                    return BadRequest();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.CreateMapPack);

                if (!canManageGameServerMaps.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create map pack for game server {GameServerId} with game type {GameType}",
                        User.XtremeIdiotsId(), gameServerId, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "MapPacks");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "MapPack");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                ViewData["GameServer"] = gameServerData;

                logger.LogInformation("Successfully loaded create map pack form for user {UserId} targeting game server {GameServerId}",
                    User.XtremeIdiotsId(), gameServerId);

                return View(new CreateMapPackViewModel { GameServerId = gameServerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating map pack form for game server {GameServerId}",
                    gameServerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", gameServerId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Creates a new map pack for a game server based on the submitted form data
        /// </summary>
        /// <param name="createMapPackViewModel">The create map pack view model containing the map pack details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to manage map packs page on success, or returns the view with validation errors</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create map packs</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMapPackViewModel createMapPackViewModel, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to create map pack for game server {GameServerId}",
                    User.XtremeIdiotsId(), createMapPackViewModel.GameServerId);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(createMapPackViewModel.GameServerId);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    logger.LogWarning("Game server {GameServerId} not found when creating map pack", createMapPackViewModel.GameServerId);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for creating map pack for game server {GameServerId}", createMapPackViewModel.GameServerId);
                    ViewData["GameServer"] = gameServerData;
                    return View(createMapPackViewModel);
                }

                var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.CreateMapPack);

                if (!canManageGameServerMaps.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create map pack for game server {GameServerId} with game type {GameType}",
                        User.XtremeIdiotsId(), createMapPackViewModel.GameServerId, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "MapPacks");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "MapPack");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var createMapPackDto = new CreateMapPackDto(createMapPackViewModel.GameServerId, createMapPackViewModel.Title, createMapPackViewModel.Description)
                {
                    GameMode = createMapPackViewModel.GameMode,
                    SyncToGameServer = createMapPackViewModel.SyncToGameServer
                };

                var createMapPackApiResponse = await repositoryApiClient.MapPacks.V1.CreateMapPack(createMapPackDto);

                if (!createMapPackApiResponse.IsSuccess)
                {
                    logger.LogWarning("Failed to create map pack for game server {GameServerId}",
                        createMapPackViewModel.GameServerId);

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
                    return View(createMapPackViewModel);
                }

                var eventTelemetry = new EventTelemetry("MapPackCreated")
                    .Enrich(User)
                    .Enrich(gameServerData)
                    .Enrich(createMapPackDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully created map pack for game server {GameServerId}",
                    User.XtremeIdiotsId(), createMapPackViewModel.GameServerId);

                this.AddAlertSuccess($"Map pack '{createMapPackViewModel.Title}' has been created successfully for {gameServerData.Title}.");

                return RedirectToAction("Manage", "MapPacks", new { id = createMapPackViewModel.GameServerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating map pack for game server {GameServerId}",
                    createMapPackViewModel.GameServerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", createMapPackViewModel.GameServerId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while creating the map pack. Please try again.");

                // Reload data for the view
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(createMapPackViewModel.GameServerId);
                if (gameServerApiResponse.IsSuccess && gameServerApiResponse.Result?.Data is not null)
                {
                    ViewData["GameServer"] = gameServerApiResponse.Result.Data;
                }

                return View(createMapPackViewModel);
            }
        }
    }
}