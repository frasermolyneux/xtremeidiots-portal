using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing game server maps, including pushing maps to remote servers and deleting maps from hosts
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessMapManagerController)]
    public class MapManagerController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<MapManagerController> logger;

        public MapManagerController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            TelemetryClient telemetryClient,
            ILogger<MapManagerController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the map management interface for a specific game server
        /// </summary>
        /// <param name="id">The game server ID to manage maps for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The map management view with server maps, remote maps, and available map packs</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to manage maps</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Manage(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to manage maps for game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    logger.LogWarning("Game server {GameServerId} not found when managing maps", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canManageGameServerMaps = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ManageMaps);

                if (!canManageGameServerMaps.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to manage maps for game server {GameServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "MapManager");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Manage");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Maps");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var getServerMapsResult = await serversApiClient.Rcon.V1.GetServerMaps(id);
                var getLoadedServerMapsFromHostResult = await serversApiClient.Maps.V1.GetLoadedServerMapsFromHost(id);
                var mapPacks = await repositoryApiClient.MapPacks.V1.GetMapPacks(null, [id], null, 0, 50, MapPacksOrder.Title);

                var mapsCollectionApiResponse = await repositoryApiClient.Maps.V1.GetMaps(
                    gameServerData.GameType,
                    getServerMapsResult.Result?.Data?.Items?.Select(m => m.MapName).ToArray(),
                    null, null, 0, 50, MapsOrder.MapNameAsc);

                var viewModel = new ManageMapsViewModel(gameServerData)
                {
                    Maps = mapsCollectionApiResponse.Result?.Data?.Items?.ToList() ?? new(),
                    ServerMaps = getLoadedServerMapsFromHostResult.Result?.Data?.Items?.ToList() ?? new(),
                    RconMaps = getServerMapsResult.Result?.Data?.Items?.ToList() ?? new(),
                    MapPacks = mapPacks.Result?.Data?.Items?.ToList() ?? new()
                };

                logger.LogInformation("Successfully loaded map management view for user {UserId} and game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error managing maps for game server {GameServerId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Pushes a map from the repository to a remote game server
        /// </summary>
        /// <param name="viewModel">The push map view model containing the map name and game server ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to the manage maps view on success, or returns appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to push maps</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PushMapToRemote(PushMapToRemoteViewModel viewModel, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to push map {MapName} to game server {GameServerId}",
                    User.XtremeIdiotsId(), viewModel.MapName, viewModel.GameServerId);

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for pushing map to remote for game server {GameServerId}", viewModel.GameServerId);
                    this.AddAlertDanger("Invalid request data. Please try again.");
                    return RedirectToAction("Manage", new { id = viewModel.GameServerId });
                }

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(viewModel.GameServerId);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    logger.LogWarning("Game server {GameServerId} not found when pushing map to remote", viewModel.GameServerId);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canPushMapToRemote = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.PushMapToRemote);

                if (!canPushMapToRemote.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to push map to remote for game server {GameServerId} in game {GameType}",
                        User.XtremeIdiotsId(), viewModel.GameServerId, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "MapManager");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "PushMapToRemote");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Map");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},MapName:{viewModel.MapName}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await serversApiClient.Maps.V1.PushServerMapToHost(gameServerData.GameServerId, viewModel.MapName!);

                logger.LogInformation("User {UserId} successfully pushed map {MapName} to game server {GameServerId}",
                    User.XtremeIdiotsId(), viewModel.MapName, viewModel.GameServerId);

                this.AddAlertSuccess($"Map '{viewModel.MapName}' has been successfully pushed to the remote server.");

                return RedirectToAction("Manage", new { id = gameServerData.GameServerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error pushing map {MapName} to game server {GameServerId}",
                    viewModel.MapName, viewModel.GameServerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", viewModel.GameServerId.ToString());
                errorTelemetry.Properties.TryAdd("MapName", viewModel.MapName ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger($"Failed to push map '{viewModel.MapName}' to the remote server. Please try again.");
                return RedirectToAction("Manage", new { id = viewModel.GameServerId });
            }
        }

        /// <summary>
        /// Deletes a map from the remote game server host
        /// </summary>
        /// <param name="model">The delete map model containing the map name and game server ID</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to the manage maps view on success, or returns appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete maps</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMapFromHost(DeleteMapFromHostModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to delete map {MapName} from game server {GameServerId}",
                    User.XtremeIdiotsId(), model.MapName, model.GameServerId);

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for deleting map from host for game server {GameServerId}", model.GameServerId);
                    this.AddAlertDanger("Invalid request data. Please try again.");
                    return RedirectToAction("Manage", new { id = model.GameServerId });
                }

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    logger.LogWarning("Game server {GameServerId} not found when deleting map from host", model.GameServerId);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canDeleteMapFromHost = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.DeleteMapFromHost);

                if (!canDeleteMapFromHost.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete map from host for game server {GameServerId} in game {GameType}",
                        User.XtremeIdiotsId(), model.GameServerId, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "MapManager");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "DeleteMapFromHost");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "Map");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType},MapName:{model.MapName}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await serversApiClient.Maps.V1.DeleteServerMapFromHost(model.GameServerId, model.MapName!);

                logger.LogInformation("User {UserId} successfully deleted map {MapName} from game server {GameServerId}",
                    User.XtremeIdiotsId(), model.MapName, model.GameServerId);

                this.AddAlertSuccess($"Map '{model.MapName}' has been successfully deleted from the remote server.");

                return RedirectToAction("Manage", new { id = model.GameServerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting map {MapName} from game server {GameServerId}",
                    model.MapName, model.GameServerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", model.GameServerId.ToString());
                errorTelemetry.Properties.TryAdd("MapName", model.MapName ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger($"Failed to delete map '{model.MapName}' from the remote server. Please try again.");
                return RedirectToAction("Manage", new { id = model.GameServerId });
            }
        }
    }
}
