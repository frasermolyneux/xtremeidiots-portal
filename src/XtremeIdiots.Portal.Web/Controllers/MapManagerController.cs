using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class MapManagerController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

        public MapManagerController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            TelemetryClient telemetryClient,
            ILogger<MapManagerController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
                    id, AuthPolicies.ManageMaps, "Manage", "Maps", cancellationToken);
                if (actionResult != null) return actionResult;

                var getServerMapsResult = await serversApiClient.Rcon.V1.GetServerMaps(id);
                var getLoadedServerMapsFromHostResult = await serversApiClient.Maps.V1.GetLoadedServerMapsFromHost(id);
                var mapPacks = await repositoryApiClient.MapPacks.V1.GetMapPacks(null, [id], null, 0, 50, MapPacksOrder.Title);

                var mapsCollectionApiResponse = await repositoryApiClient.Maps.V1.GetMaps(
                    gameServerData!.GameType,
                    getServerMapsResult.Result?.Data?.Items?.Select(m => m.MapName).ToArray(),
                    null, null, 0, 50, MapsOrder.MapNameAsc);

                var viewModel = new ManageMapsViewModel(gameServerData)
                {
                    Maps = mapsCollectionApiResponse.Result?.Data?.Items?.ToList() ?? new(),
                    ServerMaps = getLoadedServerMapsFromHostResult.Result?.Data?.Items?.ToList() ?? new(),
                    RconMaps = getServerMapsResult.Result?.Data?.Items?.ToList() ?? new(),
                    MapPacks = mapPacks.Result?.Data?.Items?.ToList() ?? new()
                };

                return View(viewModel);
            }, "ManageMaps");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var modelValidationResult = CheckModelState(viewModel);
                if (modelValidationResult != null)
                {
                    this.AddAlertDanger("Invalid request data. Please try again.");
                    return RedirectToAction("Manage", new { id = viewModel.GameServerId });
                }

                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
                    viewModel.GameServerId, AuthPolicies.PushMapToRemote, "PushMapToRemote", "Map", cancellationToken);
                if (actionResult != null) return actionResult;

                await serversApiClient.Maps.V1.PushServerMapToHost(viewModel.GameServerId, viewModel.MapName!);

                TrackSuccessTelemetry("MapPushedToRemote", "PushMapToRemote", new Dictionary<string, string>
                {
                    { "GameServerId", viewModel.GameServerId.ToString() },
                    { "MapName", viewModel.MapName ?? "Unknown" }
                });

                this.AddAlertSuccess($"Map '{viewModel.MapName}' has been successfully pushed to the remote server.");
                return RedirectToAction("Manage", new { id = viewModel.GameServerId });
            }, "PushMapToRemote");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var modelValidationResult = CheckModelState(model);
                if (modelValidationResult != null)
                {
                    this.AddAlertDanger("Invalid request data. Please try again.");
                    return RedirectToAction("Manage", new { id = model.GameServerId });
                }

                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
                    model.GameServerId, AuthPolicies.DeleteMapFromHost, "DeleteMapFromHost", "Map", cancellationToken);
                if (actionResult != null) return actionResult;

                await serversApiClient.Maps.V1.DeleteServerMapFromHost(model.GameServerId, model.MapName!);

                TrackSuccessTelemetry("MapDeletedFromHost", "DeleteMapFromHost", new Dictionary<string, string>
                {
                    { "GameServerId", model.GameServerId.ToString() },
                    { "MapName", model.MapName ?? "Unknown" }
                });

                this.AddAlertSuccess($"Map '{model.MapName}' has been successfully deleted from the remote server.");
                return RedirectToAction("Manage", new { id = model.GameServerId });
            }, "DeleteMapFromHost");
        }

        /// <summary>
        /// Helper method to get game server data and check authorization
        /// </summary>
        /// <param name="gameServerId">The game server ID</param>
        /// <param name="policy">The authorization policy to check</param>
        /// <param name="action">The action being performed</param>
        /// <param name="resourceType">The type of resource being accessed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple of ActionResult (if unauthorized or not found) and game server data</returns>
        private async Task<(IActionResult? ActionResult, Repository.Abstractions.Models.V1.GameServers.GameServerDto? GameServerData)> GetAuthorizedGameServerAsync(
            Guid gameServerId,
            string policy,
            string action,
            string resourceType,
            CancellationToken cancellationToken = default)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(gameServerId, cancellationToken);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("Game server {GameServerId} not found when {Action}", gameServerId, action);
                return (NotFound(), null);
            }

            var gameServerData = gameServerApiResponse.Result.Data;
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                gameServerData.GameType,
                policy,
                action,
                resourceType,
                $"GameType:{gameServerData.GameType},GameServerId:{gameServerId}",
                gameServerData);

            return authResult != null ? (authResult, null) : (null, gameServerData);
        }
    }
}
