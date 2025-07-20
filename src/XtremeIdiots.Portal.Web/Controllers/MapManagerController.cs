using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing game server maps, including pushing maps to remote servers and deleting maps from hosts.
/// Provides comprehensive map management functionality for Call of Duty game servers including RCON integration
/// and FTP operations for map file management across the XtremeIdiots gaming community infrastructure.
/// </summary>
/// <remarks>
/// This controller handles map management operations including:
/// - Displaying available maps on remote servers via RCON
/// - Pushing maps from repository to remote game servers via FTP
/// - Deleting maps from remote game server hosts
/// - Managing map packs and collections for organized deployment
/// - Integration with both Repository API for map metadata and Servers API for remote operations
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessMapManagerController)]
public class MapManagerController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IServersApiClient serversApiClient;

    /// <summary>
    /// Initializes a new instance of the MapManagerController with required dependencies for map management operations.
    /// </summary>
    /// <param name="authorizationService">Service for checking authorization policies and permissions</param>
    /// <param name="repositoryApiClient">Client for accessing repository API to retrieve map metadata and map packs</param>
    /// <param name="serversApiClient">Client for accessing servers API to perform RCON and FTP operations on remote game servers</param>
    /// <param name="telemetryClient">Client for tracking telemetry events and metrics</param>
    /// <param name="logger">Logger instance for structured logging</param>
    /// <param name="configuration">Application configuration settings</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
    /// Displays the comprehensive map management interface for a specific game server.
    /// Aggregates data from multiple sources including RCON server maps, hosted map files and available map packs.
    /// </summary>
    /// <param name="id">The unique identifier of the game server to manage maps for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
    /// <returns>
    /// map management view containing:
    /// - Server maps retrieved via RCON commands
    /// - Maps currently hosted on the FTP server
    /// - Available map packs for bulk deployment
    /// - Map metadata from the repository
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionmanage maps for the specified game server</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the specified game server is not found in the repository</exception>
    /// <exception cref="InvalidOperationException">Thrown when RCON or FTP operations fail due to server connectivity issues</exception>
    [HttpGet]
    public async Task<IActionResult> Manage(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
     id, AuthPolicies.ManageMaps, nameof(Manage), "Maps", cancellationToken);
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
        }, nameof(Manage));
    }

    /// <summary>
    /// Pushes a map from the repository to a remote game server via FTP.
    /// Transfers map files from the central repository to the target game server's FTP host directory
    /// and validates the operation through authorization and model validation.
    /// </summary>
    /// <param name="viewModel">The push map view model containing the map name and target game server ID with validation rules</param>
    /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
    /// <returns>
    /// Redirects to the manage maps view with success message on successful push,
    /// or returns to the manage view with error alert on validation failure or authorization denial
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionpush maps to the specified game server</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the specified game server is not found in the repository</exception>
    /// <exception cref="InvalidOperationException">Thrown when FTP operations fail due to server connectivity or file system issues</exception>
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
                return RedirectToAction(nameof(Manage), new { id = viewModel.GameServerId });
            }

            var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
     viewModel.GameServerId, AuthPolicies.PushMapToRemote, nameof(PushMapToRemote), "Map", cancellationToken);
            if (actionResult != null) return actionResult;

            await serversApiClient.Maps.V1.PushServerMapToHost(viewModel.GameServerId, viewModel.MapName!);

            TrackSuccessTelemetry("MapPushedToRemote", nameof(PushMapToRemote), new Dictionary<string, string>
        {
 { nameof(viewModel.GameServerId), viewModel.GameServerId.ToString() },
 { nameof(viewModel.MapName), viewModel.MapName ?? "Unknown" }
        });

            this.AddAlertSuccess($"Map '{viewModel.MapName}' has been successfully pushed to the remote server.");
            return RedirectToAction(nameof(Manage), new { id = viewModel.GameServerId });
        }, nameof(PushMapToRemote));
    }

    /// <summary>
    /// Deletes a map from the remote game server host via FTP operations.
    /// Removes map files from the target game server's FTP host directory and validates the operation
    /// through authorization and model validation for secure map file management.
    /// </summary>
    /// <param name="model">The delete map model containing the map name and target game server ID with validation rules</param>
    /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
    /// <returns>
    /// Redirects to the manage maps view with success message on successful deletion,
    /// or returns to the manage view with error alert on validation failure or authorization denial
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete maps from the specified game server</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the specified game server is not found in the repository</exception>
    /// <exception cref="InvalidOperationException">Thrown when FTP operations fail due to server connectivity or file system issues</exception>
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
                return RedirectToAction(nameof(Manage), new { id = model.GameServerId });
            }

            var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
     model.GameServerId, AuthPolicies.DeleteMapFromHost, nameof(DeleteMapFromHost), "Map", cancellationToken);
            if (actionResult != null) return actionResult;

            await serversApiClient.Maps.V1.DeleteServerMapFromHost(model.GameServerId, model.MapName!);

            TrackSuccessTelemetry("MapDeletedFromHost", nameof(DeleteMapFromHost), new Dictionary<string, string>
        {
 { nameof(model.GameServerId), model.GameServerId.ToString() },
 { nameof(model.MapName), model.MapName ?? "Unknown" }
        });

            this.AddAlertSuccess($"Map '{model.MapName}' has been successfully deleted from the remote server.");
            return RedirectToAction(nameof(Manage), new { id = model.GameServerId });
        }, nameof(DeleteMapFromHost));
    }

    /// <summary>
    /// Helper method to retrieve game server data and validate authorization for map management operations.
    /// Combines game server lookup with authorization policy checking to ensure secure access control.
    /// </summary>
    /// <param name="gameServerId">The unique identifier of the game server to authorize access for</param>
    /// <param name="policy">The authorization policy name to validate against user permissions</param>
    /// <param name="action">The specific action being performed for logging and telemetry tracking</param>
    /// <param name="resourceType">The type of resource being accessed for authorization context</param>
    /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
    /// <returns>
    /// A tuple containing:
    /// - ActionResult: Non-null if authorization failed or game server not found (should be returned immediately)
    /// - GameServerDto: The game server data if authorization succeeded (null if ActionResult is non-null)
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the authorization service encounters an unexpected error</exception>
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
