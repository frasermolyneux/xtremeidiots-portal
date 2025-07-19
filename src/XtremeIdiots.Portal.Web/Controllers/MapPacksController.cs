using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.MapPacks;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing map packs associated with game servers
    /// </summary>
    [Authorize(Policy = AuthPolicies.ManageMaps)]
    public class MapPacksController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;

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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
                    gameServerId,
                    AuthPolicies.CreateMapPack,
                    "Create",
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
            }, "LoadCreateMapPackForm");
        }

        /// <summary>
        /// Creates a new map pack for a game server based on the submitted form data
        /// </summary>
        /// <param name="model">The create map pack view model containing the map pack details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to manage map packs page on success, or returns the view with validation errors</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create map packs</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMapPackViewModel model, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(
                    model.GameServerId,
                    AuthPolicies.CreateMapPack,
                    "Create",
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

                TrackSuccessTelemetry("MapPackCreated", "CreateMapPack", new Dictionary<string, string>
                {
                    { "GameServerId", model.GameServerId.ToString() },
                    { "Title", model.Title },
                    { "GameType", gameServerData!.GameType.ToString() }
                });

                this.AddAlertSuccess($"Map pack '{model.Title}' has been created successfully for {gameServerData.Title}.");

                return RedirectToAction("Manage", "MapPacks", new { id = model.GameServerId });
            }, "CreateMapPack");
        }

        /// <summary>
        /// Helper method to get game server data and check authorization
        /// </summary>
        /// <param name="gameServerId">The game server ID</param>
        /// <param name="policy">The authorization policy to check</param>
        /// <param name="action">The action being performed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A tuple containing the action result if unauthorized/not found, and the game server data if authorized</returns>
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

            return authResult != null ? (authResult, null) : (null, gameServerData);
        }
    }
}