using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing game servers including creation, editing, and configuration
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessGameServers)]
    public class GameServersController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<GameServersController> logger;

        public GameServersController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<GameServersController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the list of game servers that the current user has access to view
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The index view with a list of game servers, or error response if no servers are accessible</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing game servers index", User.XtremeIdiotsId());

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameServer };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, null, 0, 50, GameServerOrder.BannerServerListPosition);

                if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve game servers for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var userId = User.XtremeIdiotsId();
                var gameServerCount = gameServersApiResponse.Result.Data.Items.Count();
                logger.LogInformation("User {UserId} successfully accessed {GameServerCount} game servers",
                    userId, gameServerCount);

                return View(gameServersApiResponse.Result.Data.Items);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing game servers index for user {UserId}", User.XtremeIdiotsId());

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
        /// Displays the creation form for a new game server
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The create game server view with an empty form</returns>
        [HttpGet]
        public IActionResult Create(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing game server creation form", User.XtremeIdiotsId());

                AddGameTypeViewData();
                return View(new GameServerViewModel());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing game server creation form for user {UserId}", User.XtremeIdiotsId());

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
        /// Creates a new game server based on the submitted form data
        /// </summary>
        /// <param name="model">The game server view model containing the server details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to index on success, or returns the view with validation errors</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create game servers</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameServerViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to create game server for {GameType}",
                    User.XtremeIdiotsId(), model.GameType);

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for creating game server for user {UserId}", User.XtremeIdiotsId());
                    AddGameTypeViewData(model.GameType);
                    return View(model);
                }

#pragma warning disable CS8604 // Possible null reference argument. // ModelState check is just above.
                var createGameServerDto = new CreateGameServerDto(model.Title, model.GameType, model.Hostname, model.QueryPort);
#pragma warning restore CS8604 // Possible null reference argument.

                var canCreateGameServer = await authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.CreateGameServer);

                if (!canCreateGameServer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create game server for {GameType}",
                        User.XtremeIdiotsId(), createGameServerDto.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "GameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{createGameServerDto.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                createGameServerDto.Title = model.Title;
                createGameServerDto.Hostname = model.Hostname;
                createGameServerDto.QueryPort = model.QueryPort;

                var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.EditGameServerFtp);

                if (canEditGameServerFtp.Succeeded)
                {
                    createGameServerDto.FtpHostname = model.FtpHostname;
                    createGameServerDto.FtpPort = model.FtpPort;
                    createGameServerDto.FtpUsername = model.FtpUsername;
                    createGameServerDto.FtpPassword = model.FtpPassword;
                }

                var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, createGameServerDto.GameType, AuthPolicies.EditGameServerRcon);

                if (canEditGameServerRcon.Succeeded)
                    createGameServerDto.RconPassword = model.RconPassword;

                createGameServerDto.LiveTrackingEnabled = model.LiveTrackingEnabled;
                createGameServerDto.BannerServerListEnabled = model.BannerServerListEnabled;
                createGameServerDto.ServerListPosition = model.ServerListPosition;
                createGameServerDto.HtmlBanner = model.HtmlBanner;
                createGameServerDto.PortalServerListEnabled = model.PortalServerListEnabled;
                createGameServerDto.ChatLogEnabled = model.ChatLogEnabled;
                createGameServerDto.BotEnabled = model.BotEnabled;

                var createResult = await repositoryApiClient.GameServers.V1.CreateGameServer(createGameServerDto);

                if (createResult.IsSuccess)
                {
                    var eventTelemetry = new EventTelemetry("GameServerCreated")
                        .Enrich(User);
                    eventTelemetry.Properties.TryAdd("GameType", createGameServerDto.GameType.ToString());
                    eventTelemetry.Properties.TryAdd("Title", createGameServerDto.Title ?? "Unknown");
                    telemetryClient.TrackEvent(eventTelemetry);

                    logger.LogInformation("User {UserId} successfully created game server for {GameType}",
                        User.XtremeIdiotsId(), model.GameType);

                    this.AddAlertSuccess($"The game server has been successfully created for {model.GameType}");

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    logger.LogWarning("Failed to create game server for user {UserId} and game type {GameType}",
                        User.XtremeIdiotsId(), model.GameType);

                    this.AddAlertDanger("Failed to create the game server. Please try again.");
                    AddGameTypeViewData(model.GameType);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating game server for user {UserId} and game type {GameType}",
                    User.XtremeIdiotsId(), model.GameType);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameType", model.GameType.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while creating the game server. Please try again.");
                AddGameTypeViewData(model.GameType);
                return View(model);
            }
        }

        /// <summary>
        /// Displays detailed information for a specific game server
        /// </summary>
        /// <param name="id">The game server ID to display details for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The details view with game server information, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to view the game server</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to view game server details {GameServerId}",
                    User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Game server {GameServerId} not found when viewing details", id);
                    return NotFound();
                }

                if (gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server data is null for {GameServerId}", id);
                    return BadRequest();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewGameServer = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewGameServer);

                if (!canViewGameServer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view game server {GameServerId} for {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "GameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Details");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
                var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

                gameServerData.ClearNoPermissionBanFileMonitors(gameTypes, banFileMonitorIds);

                logger.LogInformation("User {UserId} successfully viewed game server details {GameServerId}",
                    User.XtremeIdiotsId(), id);

                return View(gameServerData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error viewing game server details {GameServerId} for user {UserId}",
                    id, User.XtremeIdiotsId());

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
        /// Displays the edit form for modifying an existing game server
        /// </summary>
        /// <param name="id">The game server ID to edit</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The edit game server view with populated form data, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to edit the game server</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to edit game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Game server {GameServerId} not found when editing", id);
                    return NotFound();
                }

                if (gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server data is null for {GameServerId}", id);
                    return BadRequest();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                AddGameTypeViewData(gameServerData.GameType);

                var canEditGameServer = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServer);

                if (!canEditGameServer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit game server {GameServerId} for {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "GameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerFtp);

                if (!canEditGameServerFtp.Succeeded)
                    gameServerData.ClearFtpCredentials();

                var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerRcon);

                if (!canEditGameServerRcon.Succeeded)
                    gameServerData.ClearRconCredentials();

                logger.LogInformation("User {UserId} successfully accessed edit form for game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                return View(gameServerData.ToViewModel());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing edit form for game server {GameServerId} for user {UserId}",
                    id, User.XtremeIdiotsId());

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
        /// Updates an existing game server based on the submitted form data
        /// </summary>
        /// <param name="model">The game server view model containing the updated server details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to index on success, or returns the view with validation errors</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to edit the game server</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GameServerViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to update game server {GameServerId}",
                    User.XtremeIdiotsId(), model.GameServerId);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(model.GameServerId);

                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Game server {GameServerId} not found when updating", model.GameServerId);
                    return NotFound();
                }

                if (gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server data is null for {GameServerId}", model.GameServerId);
                    return BadRequest();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for updating game server {GameServerId} for user {UserId}",
                        model.GameServerId, User.XtremeIdiotsId());
                    AddGameTypeViewData(model.GameType);
                    return View(model);
                }

                var canEditGameServer = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServer);

                if (!canEditGameServer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit game server {GameServerId} for {GameType}",
                        User.XtremeIdiotsId(), model.GameServerId, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "GameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var editGameServerDto = new EditGameServerDto(gameServerData.GameServerId);

                editGameServerDto.Title = model.Title;
                editGameServerDto.Hostname = model.Hostname;
                editGameServerDto.QueryPort = model.QueryPort;

                var canEditGameServerFtp = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerFtp);

                if (canEditGameServerFtp.Succeeded)
                {
                    editGameServerDto.FtpHostname = model.FtpHostname;
                    editGameServerDto.FtpPort = model.FtpPort;
                    editGameServerDto.FtpUsername = model.FtpUsername;
                    editGameServerDto.FtpPassword = model.FtpPassword;
                }

                var canEditGameServerRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.EditGameServerRcon);

                if (canEditGameServerRcon.Succeeded)
                    editGameServerDto.RconPassword = model.RconPassword;

                editGameServerDto.LiveTrackingEnabled = model.LiveTrackingEnabled;
                editGameServerDto.BannerServerListEnabled = model.BannerServerListEnabled;
                editGameServerDto.ServerListPosition = model.ServerListPosition;
                editGameServerDto.HtmlBanner = model.HtmlBanner;
                editGameServerDto.PortalServerListEnabled = model.PortalServerListEnabled;
                editGameServerDto.ChatLogEnabled = model.ChatLogEnabled;
                editGameServerDto.BotEnabled = model.BotEnabled;

                var updateResult = await repositoryApiClient.GameServers.V1.UpdateGameServer(editGameServerDto);

                if (updateResult.IsSuccess)
                {
                    var eventTelemetry = new EventTelemetry("GameServerUpdated")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    telemetryClient.TrackEvent(eventTelemetry);

                    logger.LogInformation("User {UserId} successfully updated game server {GameServerId} for {GameType}",
                        User.XtremeIdiotsId(), gameServerData.GameServerId, gameServerData.GameType);

                    this.AddAlertSuccess($"The game server {gameServerData.Title} has been updated for {gameServerData.GameType}");

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    logger.LogWarning("Failed to update game server {GameServerId} for user {UserId}",
                        model.GameServerId, User.XtremeIdiotsId());

                    this.AddAlertDanger("Failed to update the game server. Please try again.");
                    AddGameTypeViewData(model.GameType);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating game server {GameServerId} for user {UserId}",
                    model.GameServerId, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", model.GameServerId.ToString());
                errorTelemetry.Properties.TryAdd("GameType", model.GameType.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while updating the game server. Please try again.");
                AddGameTypeViewData(model.GameType);
                return View(model);
            }
        }

        /// <summary>
        /// Displays the delete confirmation form for a game server
        /// </summary>
        /// <param name="id">The game server ID to delete</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The delete confirmation view with game server information, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete game servers</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to delete game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Game server {GameServerId} not found when deleting", id);
                    return NotFound();
                }

                if (gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server data is null for {GameServerId}", id);
                    return BadRequest();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canDeleteGameServer = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);

                if (!canDeleteGameServer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete game server {GameServerId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "GameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully accessed delete confirmation for game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                return View(gameServerData.ToViewModel());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing delete confirmation for game server {GameServerId} for user {UserId}",
                    id, User.XtremeIdiotsId());

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
        /// Permanently deletes a game server after confirmation
        /// </summary>
        /// <param name="id">The game server ID to delete</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to index on success, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete game servers</exception>
        /// <exception cref="KeyNotFoundException">Thrown when game server is not found</exception>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to confirm deletion of game server {GameServerId}",
                    User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound)
                {
                    logger.LogWarning("Game server {GameServerId} not found when confirming deletion", id);
                    return NotFound();
                }

                if (gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server data is null for {GameServerId}", id);
                    return BadRequest();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canDeleteGameServer = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteGameServer);

                if (!canDeleteGameServer.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete game server {GameServerId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "GameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var deleteResult = await repositoryApiClient.GameServers.V1.DeleteGameServer(id);

                if (deleteResult.IsSuccess)
                {
                    var eventTelemetry = new EventTelemetry("GameServerDeleted")
                        .Enrich(User)
                        .Enrich(gameServerData);
                    telemetryClient.TrackEvent(eventTelemetry);

                    logger.LogInformation("User {UserId} successfully deleted game server {GameServerId} for {GameType}",
                        User.XtremeIdiotsId(), gameServerData.GameServerId, gameServerData.GameType);

                    this.AddAlertSuccess($"The game server {gameServerData.Title} has been deleted for {gameServerData.GameType}");

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    logger.LogWarning("Failed to delete game server {GameServerId} for user {UserId}",
                        id, User.XtremeIdiotsId());

                    this.AddAlertDanger("Failed to delete the game server. Please try again.");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting game server {GameServerId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while deleting the game server. Please try again.");
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Adds game type options to ViewData for form dropdown selection
        /// </summary>
        /// <param name="selected">The currently selected game type, defaults to Unknown if not specified</param>
        private void AddGameTypeViewData(GameType? selected = null)
        {
            try
            {
                if (selected == null)
                    selected = GameType.Unknown;

                var gameTypes = User.GetGameTypesForGameServers();
                ViewData["GameType"] = new SelectList(gameTypes, selected);

                logger.LogDebug("Added {GameTypeCount} game types to ViewData with {SelectedGameType} selected",
                    gameTypes.Count(), selected);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding game type ViewData for user {UserId}", User.XtremeIdiotsId());

                // Fallback to empty list to prevent view errors
                ViewData["GameType"] = new SelectList(Enumerable.Empty<GameType>(), selected ?? GameType.Unknown);
            }
        }
    }
}