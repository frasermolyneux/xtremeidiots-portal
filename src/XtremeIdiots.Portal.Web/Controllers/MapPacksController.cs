using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Manages map pack creation and organization for game servers
/// </summary>
/// <remarks>
/// Initializes a new instance of the MapPacksController
/// </remarks>
/// <param name="authorizationService">Service for checking user authorization</param>
/// <param name="repositoryApiClient">Client for accessing repository data</param>
/// <param name="telemetryClient">Client for tracking telemetry data</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.ManageMaps)]
public class MapPacksController(
    IAuthorizationService authorizationService,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<MapPacksController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the create map pack form for a specific game server
    /// </summary>
    /// <param name="gameServerId">Game server identifier</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with create map pack form or NotFound if server doesn't exist</returns>
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

            if (actionResult != null)
                return actionResult;

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
    /// Creates a new map pack for the specified game server
    /// </summary>
    /// <param name="model">Map pack creation model with title, description, and game mode</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirect to MapManager on success or view with validation errors</returns>
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

            if (actionResult != null)
                return actionResult;

            var modelValidationResult = CheckModelState(model, m => ViewData["GameServer"] = gameServerData);
            if (modelValidationResult != null)
                return modelValidationResult;

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