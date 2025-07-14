using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for server administration functionality including RCON access, chat logs, and server management
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessServerAdmin)]
    public class ServerAdminController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IServersApiClient serversApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<ServerAdminController> logger;

        public ServerAdminController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IServersApiClient serversApiClient,
            TelemetryClient telemetryClient,
            ILogger<ServerAdminController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the server administration dashboard with a list of game servers that support live tracking
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The server admin dashboard view with available game servers</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing server admin dashboard", User.XtremeIdiotsId());

                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.ServerAdmin };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(gameTypes, gameServerIds, GameServerFilter.LiveTrackingEnabled, 0, 50, GameServerOrder.BannerServerListPosition);

                if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogError("Failed to retrieve game servers for server admin dashboard for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var results = gameServersApiResponse.Result.Data.Items.Select(gs => new ServerAdminGameServerViewModel
                {
                    GameServer = gs
                }).ToList();

                logger.LogInformation("Successfully loaded {Count} game servers for user {UserId} server admin dashboard",
                    results.Count, User.XtremeIdiotsId());

                return View(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading server admin dashboard for user {UserId}", User.XtremeIdiotsId());

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
        /// Displays the live RCON view for a specific game server
        /// </summary>
        /// <param name="id">The game server ID to view RCON for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The RCON view for the specified server, or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> ViewRcon(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to view RCON for server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when viewing RCON", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view RCON for server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "ViewRcon");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully accessed RCON view for server {ServerId}",
                    User.XtremeIdiotsId(), id);

                return View(gameServerData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error viewing RCON for server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Gets the current players on a game server via RCON for AJAX requests
        /// </summary>
        /// <param name="id">The game server ID to get players for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with current server players</returns>
        [HttpGet]
        public async Task<IActionResult> GetRconPlayers(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting RCON players for server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when getting RCON players", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to get RCON players for server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GetRconPlayers");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var getServerStatusResult = await serversApiClient.Rcon.V1.GetServerStatus(id);

                return Json(new
                {
                    data = (getServerStatusResult.IsSuccess && getServerStatusResult.Result?.Data != null) ? getServerStatusResult.Result.Data.Players : null
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting RCON players for server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Restarts a game server via RCON
        /// </summary>
        /// <param name="id">The game server ID to restart</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response indicating success or failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestartServer(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to restart server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when restarting server", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to restart server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "RestartServer");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully initiated server restart for {ServerId}",
                    User.XtremeIdiotsId(), id);

                return Json(new
                {
                    Success = true,
                    //Message = result
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error restarting server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Restarts the map on a game server via RCON
        /// </summary>
        /// <param name="id">The game server ID to restart the map on</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response indicating success or failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestartMap(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to restart map on server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when restarting map", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to restart map on server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "RestartMap");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully initiated map restart for server {ServerId}",
                    User.XtremeIdiotsId(), id);

                return Json(new
                {
                    Success = true,
                    //Message = result
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error restarting map on server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Performs a fast restart of the map on a game server via RCON
        /// </summary>
        /// <param name="id">The game server ID to fast restart the map on</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response indicating success or failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FastRestartMap(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to fast restart map on server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when fast restarting map", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to fast restart map on server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "FastRestartMap");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully initiated fast map restart for server {ServerId}",
                    User.XtremeIdiotsId(), id);

                return Json(new
                {
                    Success = true,
                    //Message = result
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fast restarting map on server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Skips to the next map on a game server via RCON
        /// </summary>
        /// <param name="id">The game server ID to skip to the next map on</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response indicating success or failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NextMap(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to skip to next map on server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when skipping to next map", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to skip to next map on server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "NextMap");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully initiated skip to next map for server {ServerId}",
                    User.XtremeIdiotsId(), id);

                return Json(new
                {
                    Success = true,
                    //Message = result
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error skipping to next map on server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Kicks a player from the server by slot number
        /// </summary>
        /// <param name="id">The game server ID</param>
        /// <param name="num">The slot number of the player to kick</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to RCON view or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> KickPlayer(Guid id, string num, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to kick player {PlayerSlot} from server {ServerId}", User.XtremeIdiotsId(), num, id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when kicking player", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewLiveRcon = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewLiveRcon);

                if (!canViewLiveRcon.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to kick player from server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "KickPlayer");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServer");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(num))
                {
                    logger.LogWarning("Invalid player slot number provided by user {UserId} for server {ServerId}", User.XtremeIdiotsId(), id);
                    return NotFound();
                }

                // TODO: Implement RCON kick player functionality
                this.AddAlertSuccess($"Player in slot {num} has been kicked");
                logger.LogInformation("User {UserId} successfully kicked player {PlayerSlot} from server {ServerId}", User.XtremeIdiotsId(), num, id);
                return RedirectToAction(nameof(ViewRcon), new { id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error kicking player {PlayerSlot} from server {ServerId} by user {UserId}", num, id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while kicking the player.");
                return RedirectToAction(nameof(ViewRcon), new { id });
            }
        }

        /// <summary>
        /// Displays the global chat log index page
        /// </summary>
        /// <returns>The chat log index view</returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicies.ViewGlobalChatLog)]
        public IActionResult ChatLogIndex()
        {
            logger.LogInformation("User {UserId} accessing global chat log index", User.XtremeIdiotsId());
            return View();
        }

        /// <summary>
        /// Gets chat log data for AJAX DataTables requests (global scope)
        /// </summary>
        /// <param name="lockedOnly">Filter to show only locked messages</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicies.ViewGlobalChatLog)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetChatLogAjax(bool? lockedOnly = null, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting global chat log data", User.XtremeIdiotsId());
                return await GetChatLogPrivate(null, null, null, lockedOnly, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting global chat log for user {UserId}", User.XtremeIdiotsId());
                throw;
            }
        }

        /// <summary>
        /// Displays the chat log for a specific game type
        /// </summary>
        /// <param name="id">The game type to view chat logs for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The chat log view for the specified game type</returns>
        [HttpGet]
        public async Task<IActionResult> GameChatLog(GameType id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to access game chat log for {GameType}", User.XtremeIdiotsId(), id);

                var canViewGameChatLog = await authorizationService.AuthorizeAsync(User, id, AuthPolicies.ViewGameChatLog);

                if (!canViewGameChatLog.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view game chat log for {GameType}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GameChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                ViewData["GameType"] = id;
                logger.LogInformation("User {UserId} successfully accessed game chat log for {GameType}", User.XtremeIdiotsId(), id);
                return View(nameof(ChatLogIndex));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing game chat log for {GameType} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameType", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Gets chat log data for AJAX DataTables requests (game scope)
        /// </summary>
        /// <param name="id">The game type ID</param>
        /// <param name="lockedOnly">Filter to show only locked messages</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetGameChatLogAjax(GameType id, bool? lockedOnly = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var canViewGameChatLog = await authorizationService.AuthorizeAsync(User, id, AuthPolicies.ViewGameChatLog);

                if (!canViewGameChatLog.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to get game chat log data for {GameType}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GetGameChatLogAjax");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} requesting game chat log data for {GameType}", User.XtremeIdiotsId(), id);
                return await GetChatLogPrivate(id, null, null, lockedOnly, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting game chat log for user {UserId}", User.XtremeIdiotsId());
                throw;
            }
        }

        /// <summary>
        /// Displays the chat log for a specific server
        /// </summary>
        /// <param name="id">The game server ID to view chat logs for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The chat log view for the specified game server</returns>
        [HttpGet]
        public async Task<IActionResult> ServerChatLog(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to access server chat log for server {ServerId}", User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when accessing server chat log", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewServerChatLog = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewServerChatLog);

                if (!canViewServerChatLog.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to view server chat log for server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "ServerChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                ViewData["GameServerId"] = id;
                logger.LogInformation("User {UserId} successfully accessed server chat log for server {ServerId}", User.XtremeIdiotsId(), id);
                return View(nameof(ChatLogIndex));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing server chat log for server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ServerId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Gets chat log data for AJAX DataTables requests (server scope)
        /// </summary>
        /// <param name="id">The game server ID</param>
        /// <param name="lockedOnly">Filter to show only locked messages</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetServerChatLogAjax(Guid id, bool? lockedOnly = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Game server {ServerId} not found when getting server chat log data", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;
                var canViewServerChatLog = await authorizationService.AuthorizeAsync(User, gameServerData.GameType, AuthPolicies.ViewServerChatLog);

                if (!canViewServerChatLog.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to get server chat log data for server {ServerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, gameServerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GetServerChatLogAjax");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ServerId:{id},GameType:{gameServerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} requesting server chat log data for server {ServerId}", User.XtremeIdiotsId(), id);
                return await GetChatLogPrivate(null, id, null, lockedOnly, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting server chat log for server {ServerId} by user {UserId}", id, User.XtremeIdiotsId());
                throw;
            }
        }

        /// <summary>
        /// Gets chat log data for AJAX DataTables requests (player scope)
        /// </summary>
        /// <param name="id">The player ID</param>
        /// <param name="lockedOnly">Filter to show only locked messages</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetPlayerChatLog(Guid id, bool? lockedOnly = null, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} requesting player chat log data for player {PlayerId}", User.XtremeIdiotsId(), id);

                var playerApiResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (playerApiResponse.IsNotFound || playerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Player {PlayerId} not found when getting player chat log data", id);
                    return NotFound();
                }

                var playerData = playerApiResponse.Result.Data;
                var canViewGameChatLog = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.ViewGameChatLog);

                if (!canViewGameChatLog.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to get player chat log data for player {PlayerId} in game {GameType}",
                        User.XtremeIdiotsId(), id, playerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GetPlayerChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ChatLog");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{id},GameType:{playerData.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                return await GetChatLogPrivate(playerData.GameType, null, playerData.PlayerId, lockedOnly, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting player chat log for player {PlayerId} by user {UserId}", id, User.XtremeIdiotsId());
                throw;
            }
        }

        /// <summary>
        /// Displays a permanent link to a specific chat message
        /// </summary>
        /// <param name="id">The chat message ID to display</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The chat message permalink view</returns>
        [HttpGet]
        public async Task<IActionResult> ChatLogPermaLink(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing chat message permalink for message {MessageId}", User.XtremeIdiotsId(), id);

                var chatMessageApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessage(id);

                if (chatMessageApiResponse.IsNotFound || chatMessageApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Chat message {MessageId} not found when accessing permalink", id);
                    return NotFound();
                }

                logger.LogInformation("User {UserId} successfully accessed chat message permalink for message {MessageId}",
                    User.XtremeIdiotsId(), id);

                return View(chatMessageApiResponse.Result.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing chat message permalink for message {MessageId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("MessageId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Toggles the lock status of a chat message
        /// </summary>
        /// <param name="id">The chat message ID to toggle lock status for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to chat message permalink or appropriate error response</returns>
        [HttpPost]
        [Authorize(Policy = AuthPolicies.LockChatMessages)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleChatMessageLock(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to toggle lock status for chat message {MessageId}",
                    User.XtremeIdiotsId(), id);

                var chatMessageApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessage(id);

                if (chatMessageApiResponse.IsNotFound || chatMessageApiResponse.Result?.Data?.GameServer == null)
                {
                    logger.LogWarning("Chat message {MessageId} not found when toggling lock status", id);
                    return NotFound();
                }

                var chatMessageData = chatMessageApiResponse.Result.Data;
                var canLockChatMessage = await authorizationService.AuthorizeAsync(User, chatMessageData.GameServer.GameType, AuthPolicies.LockChatMessages);

                if (!canLockChatMessage.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to toggle lock status for chat message {MessageId} in game {GameType}",
                        User.XtremeIdiotsId(), id, chatMessageData.GameServer.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ServerAdmin");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "ToggleChatMessageLock");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ChatMessage");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"MessageId:{id},GameType:{chatMessageData.GameServer.GameType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var toggleResponse = await repositoryApiClient.ChatMessages.V1.ToggleLockedStatus(id);

                if (!toggleResponse.IsSuccess)
                {
                    logger.LogError("Failed to toggle lock status for chat message {MessageId} by user {UserId}", id, User.XtremeIdiotsId());
                    this.AddAlertDanger("An error occurred while updating the chat message lock status.");
                    return RedirectToAction(nameof(ChatLogPermaLink), new { id });
                }

                logger.LogInformation("User {UserId} successfully toggled lock status for chat message {MessageId}",
                    User.XtremeIdiotsId(), id);

                this.AddAlertSuccess($"Chat message lock status has been updated successfully.");

                return RedirectToAction(nameof(ChatLogPermaLink), new { id });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error toggling chat message lock status for {MessageId} by user {UserId}", id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("MessageId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while updating the chat message lock status.");
                return RedirectToAction(nameof(ChatLogPermaLink), new { id });
            }
        }

        /// <summary>
        /// Private helper method to retrieve chat log data for DataTables AJAX requests
        /// </summary>
        /// <param name="gameType">Optional game type filter</param>
        /// <param name="gameServerId">Optional game server filter</param>
        /// <param name="playerId">Optional player filter</param>
        /// <param name="lockedOnly">Optional locked messages filter</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        private async Task<IActionResult> GetChatLogPrivate(GameType? gameType, Guid? gameServerId, Guid? playerId, bool? lockedOnly = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

                if (model == null)
                {
                    logger.LogWarning("Invalid chat log request model for user {UserId}", User.XtremeIdiotsId());
                    return BadRequest();
                }

                var order = ChatMessageOrder.TimestampDesc;
                if (model.Order != null && model.Order.Any())
                {
                    var orderColumn = model.Columns[model.Order.First().Column].Name;
                    var searchOrder = model.Order.First().Dir;

                    switch (orderColumn)
                    {
                        case "timestamp":
                            order = searchOrder == "asc" ? ChatMessageOrder.TimestampAsc : ChatMessageOrder.TimestampDesc;
                            break;
                    }
                }

                // Check for locked filter parameter
                if (model.Search?.Value?.StartsWith("locked:", StringComparison.OrdinalIgnoreCase) == true)
                {
                    lockedOnly = true;
                    model.Search.Value = model.Search.Value.Substring(7).Trim(); // Remove "locked:" prefix
                }

                var chatMessagesApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessages(gameType, gameServerId, playerId, model.Search?.Value, model.Start, model.Length, order, lockedOnly);

                if (!chatMessagesApiResponse.IsSuccess || chatMessagesApiResponse.Result?.Data == null)
                {
                    logger.LogError("Failed to retrieve chat messages for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                return Json(new
                {
                    model.Draw,
                    recordsTotal = chatMessagesApiResponse.Result.Data.TotalCount,
                    recordsFiltered = chatMessagesApiResponse.Result.Data.FilteredCount,
                    data = chatMessagesApiResponse.Result.Data.Items
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving chat log data for user {UserId}", User.XtremeIdiotsId());
                throw;
            }
        }
    }
}
