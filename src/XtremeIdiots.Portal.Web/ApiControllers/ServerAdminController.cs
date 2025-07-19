using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers
{
    /// <summary>
    /// API controller for server administration endpoints that provide JSON data
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessServerAdmin)]
    [Route("ServerAdmin")]
    public class ServerAdminController : BaseApiController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public ServerAdminController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<ServerAdminController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Returns chat log data as JSON for global chat log DataTable
        /// </summary>
        /// <param name="lockedOnly">Whether to filter for locked entries only</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost("GetChatLogAjax")]
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
        /// Returns chat log data as JSON for game-specific chat log DataTable
        /// </summary>
        /// <param name="id">The game type to filter chat logs for</param>
        /// <param name="lockedOnly">Whether to filter for locked entries only</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost("GetGameChatLogAjax")]
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
        /// Returns chat log data as JSON for server-specific chat log DataTable
        /// </summary>
        /// <param name="id">The server ID to filter chat logs for</param>
        /// <param name="lockedOnly">Whether to filter for locked entries only</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response with chat log data</returns>
        [HttpPost("GetServerChatLogAjax")]
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
        /// Private method to handle chat log data retrieval for various filters
        /// </summary>
        /// <param name="gameType">Optional game type filter</param>
        /// <param name="serverId">Optional server ID filter</param>
        /// <param name="playerId">Optional player ID filter</param>
        /// <param name="lockedOnly">Whether to filter for locked entries only</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data formatted for DataTable consumption</returns>
        private async Task<IActionResult> GetChatLogPrivate(GameType? gameType, Guid? serverId, Guid? playerId, bool? lockedOnly, CancellationToken cancellationToken = default)
        {
            var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

            if (model is null)
            {
                Logger.LogWarning("Invalid request model for chat log AJAX from user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

            var order = ChatMessageOrder.TimestampDesc;

            if (model.Order?.Any() == true)
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

            var chatMessagesApiResponse = await repositoryApiClient.ChatMessages.V1.GetChatMessages(
                gameType, serverId, playerId, model.Search?.Value,
                model.Start, model.Length, order, lockedOnly, cancellationToken);

            if (!chatMessagesApiResponse.IsSuccess || chatMessagesApiResponse.Result?.Data is null)
            {
                Logger.LogError("Failed to retrieve chat log for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500, "Failed to retrieve chat log data");
            }

            TrackSuccessTelemetry("ChatLogLoaded", "GetChatLog", new Dictionary<string, string>
            {
                { "GameType", gameType?.ToString() ?? "All" },
                { "ServerId", serverId?.ToString() ?? "All" },
                { "PlayerId", playerId?.ToString() ?? "All" },
                { "LockedOnly", lockedOnly?.ToString() ?? "All" },
                { "ResultCount", chatMessagesApiResponse.Result.Data.Items?.Count().ToString() ?? "0" }
            });

            return Ok(new
            {
                model.Draw,
                recordsTotal = chatMessagesApiResponse.Result.Data.TotalCount,
                recordsFiltered = chatMessagesApiResponse.Result.Data.FilteredCount,
                data = chatMessagesApiResponse.Result.Data.Items
            });
        }
    }
}
