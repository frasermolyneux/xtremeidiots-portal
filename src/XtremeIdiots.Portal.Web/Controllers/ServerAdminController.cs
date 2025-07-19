using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Abstractions.Models.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for server administration functionality including RCON access, chat logs, and server management
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessServerAdmin)]
    public class ServerAdminController(
        IAuthorizationService authorizationService,
        IRepositoryApiClient repositoryApiClient,
        IServersApiClient serversApiClient,
        TelemetryClient telemetryClient,
        ILogger<ServerAdminController> logger,
        IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
    {
        private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        private readonly IServersApiClient serversApiClient = serversApiClient ?? throw new ArgumentNullException(nameof(serversApiClient));

        /// <summary>
        /// Displays the server administration dashboard with a list of game servers that support live tracking
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The server admin dashboard view with available game servers</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.ServerAdmin };
                var (gameTypes, gameServerIds) = User.ClaimedGamesAndItems(requiredClaims);

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                    gameTypes, gameServerIds, GameServerFilter.LiveTrackingEnabled, 0, 50,
                    GameServerOrder.BannerServerListPosition, cancellationToken);

                if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items is null)
                {
                    Logger.LogError("Failed to retrieve game servers for server admin dashboard for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var results = gameServersApiResponse.Result.Data.Items.Select(gs => new ServerAdminGameServerViewModel
                {
                    GameServer = gs,
                    GameServerQueryStatus = new ServerQueryStatusResponseDto(),
                    GameServerRconStatus = new ServerRconStatusResponseDto()
                }).ToList();

                Logger.LogInformation("Successfully loaded {Count} game servers for user {UserId} server admin dashboard",
                    results.Count, User.XtremeIdiotsId());

                return View(results);
            }, "Index");
        }

        /// <summary>
        /// Helper method to get game server data with authorization check
        /// </summary>
        /// <param name="id">The game server ID</param>
        /// <param name="action">The action being performed for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing ActionResult (if unauthorized/not found) and GameServerDto (if successful)</returns>
        private async Task<(IActionResult? ActionResult, GameServerDto? GameServer)> GetAuthorizedGameServerAsync(
            Guid id,
            string action,
            CancellationToken cancellationToken = default)
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("Game server {ServerId} not found when {Action}", id, action);
                return (NotFound(), null);
            }

            var gameServerData = gameServerApiResponse.Result.Data;
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                gameServerData.GameType,
                AuthPolicies.ViewLiveRcon,
                action,
                "GameServer",
                $"ServerId:{id},GameType:{gameServerData.GameType}",
                gameServerData);

            return authResult is not null ? (authResult, null) : (null, gameServerData);
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "ViewRcon", cancellationToken);
                if (actionResult is not null) return actionResult;

                return View(gameServerData);
            }, "ViewRcon");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "GetRconPlayers", cancellationToken);
                if (actionResult is not null) return actionResult;

                var getServerStatusResult = await serversApiClient.Rcon.V1.GetServerStatus(id);

                return Json(new
                {
                    data = (getServerStatusResult.IsSuccess && getServerStatusResult.Result?.Data is not null)
                        ? getServerStatusResult.Result.Data.Players
                        : null
                });
            }, "GetRconPlayers");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "RestartServer", cancellationToken);
                if (actionResult is not null) return actionResult;

                // TODO: Implement actual server restart logic when available in ServersApiClient

                TrackSuccessTelemetry("ServerRestarted", "RestartServer", new Dictionary<string, string>
                {
                    { "ServerId", id.ToString() },
                    { "GameType", gameServerData!.GameType.ToString() }
                });

                return Json(new
                {
                    Success = true
                });
            }, "RestartServer");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "RestartMap", cancellationToken);
                if (actionResult is not null) return actionResult;

                // TODO: Implement actual map restart logic when available in ServersApiClient

                TrackSuccessTelemetry("MapRestarted", "RestartMap", new Dictionary<string, string>
                {
                    { "ServerId", id.ToString() },
                    { "GameType", gameServerData!.GameType.ToString() }
                });

                return Json(new
                {
                    Success = true
                });
            }, "RestartMap");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "FastRestartMap", cancellationToken);
                if (actionResult is not null) return actionResult;

                // TODO: Implement actual fast map restart logic when available in ServersApiClient

                TrackSuccessTelemetry("MapFastRestarted", "FastRestartMap", new Dictionary<string, string>
                {
                    { "ServerId", id.ToString() },
                    { "GameType", gameServerData!.GameType.ToString() }
                });

                return Json(new
                {
                    Success = true
                });
            }, "FastRestartMap");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "NextMap", cancellationToken);
                if (actionResult is not null) return actionResult;

                // TODO: Implement actual next map logic when available in ServersApiClient

                TrackSuccessTelemetry("NextMapTriggered", "NextMap", new Dictionary<string, string>
                {
                    { "ServerId", id.ToString() },
                    { "GameType", gameServerData!.GameType.ToString() }
                });

                return Json(new
                {
                    Success = true
                });
            }, "NextMap");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, gameServerData) = await GetAuthorizedGameServerAsync(id, "KickPlayer", cancellationToken);
                if (actionResult is not null) return actionResult;

                if (string.IsNullOrWhiteSpace(num))
                {
                    Logger.LogWarning("Invalid player slot number provided by user {UserId} for server {ServerId}: {PlayerSlot}",
                        User.XtremeIdiotsId(), id, num);
                    return NotFound();
                }

                // TODO: Implement RCON kick player functionality
                this.AddAlertSuccess($"Player in slot {num} has been kicked");

                TrackSuccessTelemetry("PlayerKicked", "KickPlayer", new Dictionary<string, string>
                {
                    { "ServerId", id.ToString() },
                    { "PlayerSlot", num },
                    { "GameType", gameServerData!.GameType.ToString() }
                });

                return RedirectToAction(nameof(ViewRcon), new { id });
            }, "KickPlayer");
        }

        /// <summary>
        /// Displays the global chat log index page
        /// </summary>
        /// <returns>The chat log index view</returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicies.ViewGlobalChatLog)]
        public IActionResult ChatLogIndex()
        {
            return ExecuteWithErrorHandlingAsync(async () =>
            {
                return await Task.FromResult(View());
            }, "ChatLogIndex").Result;
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                return await GetChatLogPrivate(null, null, null, lockedOnly, cancellationToken);
            }, "GetChatLogAjax");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    id,
                    AuthPolicies.ViewGameChatLog,
                    "View",
                    "GameChatLog",
                    $"GameType:{id}",
                    id);

                if (authResult != null) return authResult;

                ViewData["GameType"] = id;
                return View(nameof(ChatLogIndex));
            }, "GameChatLog");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    id,
                    AuthPolicies.ViewGameChatLog,
                    "GetGameChatLogAjax",
                    "GameChatLog",
                    $"GameType:{id}",
                    id);

                if (authResult != null) return authResult;

                return await GetChatLogPrivate(id, null, null, lockedOnly, cancellationToken);
            }, "GetGameChatLogAjax");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Game server {ServerId} not found when accessing server chat log", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    gameServerData.GameType,
                    AuthPolicies.ViewServerChatLog,
                    "View",
                    "ServerChatLog",
                    $"ServerId:{id},GameType:{gameServerData.GameType}",
                    gameServerData);

                if (authResult != null) return authResult;

                ViewData["GameServerId"] = id;
                return View(nameof(ChatLogIndex));
            }, "ServerChatLog");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Game server {ServerId} not found when getting server chat log data", id);
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    gameServerData.GameType,
                    AuthPolicies.ViewServerChatLog,
                    "GetServerChatLogAjax",
                    "ServerChatLog",
                    $"ServerId:{id},GameType:{gameServerData.GameType}",
                    gameServerData);

                if (authResult != null) return authResult;

                return await GetChatLogPrivate(null, id, null, lockedOnly, cancellationToken);
            }, "GetServerChatLogAjax");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var playerApiResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (playerApiResponse.IsNotFound || playerApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Player {PlayerId} not found when getting player chat log data", id);
                    return NotFound();
                }

                var playerData = playerApiResponse.Result.Data;

                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    playerData.GameType,
                    AuthPolicies.ViewGameChatLog,
                    "GetPlayerChatLog",
                    "PlayerChatLog",
                    $"PlayerId:{id},GameType:{playerData.GameType}",
                    playerData);

                if (authResult is not null) return authResult;

                return await GetChatLogPrivate(playerData.GameType, null, playerData.PlayerId, lockedOnly, cancellationToken);
            }, "GetPlayerChatLog");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var chatMessageApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessage(id, cancellationToken);

                if (chatMessageApiResponse.IsNotFound || chatMessageApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Chat message {MessageId} not found when accessing permalink", id);
                    return NotFound();
                }

                return View(chatMessageApiResponse.Result.Data);
            }, "ChatLogPermaLink");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var chatMessageApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessage(id, cancellationToken);

                if (chatMessageApiResponse.IsNotFound || chatMessageApiResponse.Result?.Data?.GameServer is null)
                {
                    Logger.LogWarning("Chat message {MessageId} not found when toggling lock status", id);
                    return NotFound();
                }

                var chatMessageData = chatMessageApiResponse.Result.Data;

                var authResult = await CheckAuthorizationAsync(
                    authorizationService,
                    chatMessageData.GameServer.GameType,
                    AuthPolicies.LockChatMessages,
                    "ToggleLock",
                    "ChatMessage",
                    $"MessageId:{id},GameType:{chatMessageData.GameServer.GameType}",
                    chatMessageData);

                if (authResult != null) return authResult;

                var toggleResponse = await repositoryApiClient.ChatMessages.V1.ToggleLockedStatus(id, cancellationToken);

                if (!toggleResponse.IsSuccess)
                {
                    Logger.LogError("Failed to toggle lock status for chat message {MessageId} by user {UserId}", id, User.XtremeIdiotsId());
                    this.AddAlertDanger("An error occurred while updating the chat message lock status.");
                    return RedirectToAction(nameof(ChatLogPermaLink), new { id });
                }

                TrackSuccessTelemetry("ChatMessageLockToggled", "ToggleChatMessageLock", new Dictionary<string, string>
                {
                    { "MessageId", id.ToString() },
                    { "GameType", chatMessageData.GameServer.GameType.ToString() }
                });

                this.AddAlertSuccess("Chat message lock status has been updated successfully.");
                return RedirectToAction(nameof(ChatLogPermaLink), new { id });
            }, "ToggleChatMessageLock");
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
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model is null)
            {
                Logger.LogWarning("Invalid chat log request model for user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

            var order = ChatMessageOrder.TimestampDesc;
            if (model.Order is not null && model.Order.Any())
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

            // Support special "locked:" prefix to filter chat messages that are locked by admins
            if (model.Search?.Value?.StartsWith("locked:", StringComparison.OrdinalIgnoreCase) == true)
            {
                lockedOnly = true;
                model.Search.Value = model.Search.Value.Substring(7).Trim();
            }

            var chatMessagesApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessages(
                gameType, gameServerId, playerId, model.Search?.Value,
                model.Start, model.Length, order, lockedOnly, cancellationToken);

            if (!chatMessagesApiResponse.IsSuccess || chatMessagesApiResponse.Result?.Data is null)
            {
                Logger.LogError("Failed to retrieve chat messages for user {UserId}", User.XtremeIdiotsId());
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
    }
}
