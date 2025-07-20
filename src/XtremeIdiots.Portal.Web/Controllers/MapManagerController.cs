using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Manages server map operations including uploading, deleting, and organizing maps for game servers
/// </summary>
[Authorize(Policy = AuthPolicies.AccessMapManagerController)]
public class MapManagerController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IServersApiClient serversApiClient;

    /// <summary>
    /// Initializes a new instance of the MapManagerController
    /// </summary>
    /// <param name="authorizationService">Service for checking user authorization</param>
    /// <param name="repositoryApiClient">Client for accessing repository data</param>
    /// <param name="serversApiClient">Client for server integration operations</param>
    /// <param name="telemetryClient">Client for tracking telemetry data</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
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
    /// <param name="id">Game server identifier</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with map management options or NotFound if server doesn't exist</returns>
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
    /// Pushes a map from the local repository to the remote game server
    /// </summary>
    /// <param name="viewModel">View model containing the map and server details</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirect to Manage action with success/error message</returns>
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
    /// Deletes a map from the remote game server host
    /// </summary>
    /// <param name="model">Model containing the map and server details</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirect to Manage action with success/error message</returns>
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