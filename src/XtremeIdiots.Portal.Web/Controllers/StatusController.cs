using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for displaying status information about system components
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessStatus)]
    public class StatusController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public StatusController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<StatusController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Displays the status of ban file monitors that the current user has access to
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The ban file status view with monitor data, or appropriate error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to view ban file status</exception>
        [HttpGet]
        public async Task<IActionResult> BanFileStatus(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                // Get user's claimed games and ban file monitor access
                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
                var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

                Logger.LogInformation("User {UserId} has access to {GameTypeCount} game types and {MonitorCount} ban file monitors",
                    User.XtremeIdiotsId(), gameTypes.Count(), banFileMonitorIds.Count());

                // Retrieve ban file monitors
                var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(
                    gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition, cancellationToken);

                if (banFileMonitorsApiResponse.IsNotFound || banFileMonitorsApiResponse.Result?.Data?.Items is null)
                {
                    Logger.LogWarning("No ban file monitors found for user {UserId}", User.XtremeIdiotsId());
                    return View(new List<EditBanFileMonitorViewModel>());
                }

                var models = new List<EditBanFileMonitorViewModel>();

                // Process each ban file monitor and enrich with game server data
                foreach (var banFileMonitor in banFileMonitorsApiResponse.Result.Data.Items)
                {
                    var (actionResult, gameServerData) = await GetGameServerDataAsync(banFileMonitor.GameServerId, banFileMonitor.BanFileMonitorId, cancellationToken);

                    if (actionResult is not null)
                    {
                        // Log warning but continue processing other monitors
                        continue;
                    }

                    if (gameServerData is not null)
                    {
                        models.Add(new EditBanFileMonitorViewModel
                        {
                            BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                            FilePath = banFileMonitor.FilePath,
                            RemoteFileSize = banFileMonitor.RemoteFileSize,
                            LastSync = banFileMonitor.LastSync,
                            GameServerId = banFileMonitor.GameServerId,
                            GameServer = gameServerData
                        });
                    }
                }

                TrackSuccessTelemetry("BanFileStatusRetrieved", "BanFileStatus", new Dictionary<string, string>
                {
                    { "MonitorCount", models.Count.ToString() },
                    { "GameTypeCount", gameTypes.Count().ToString() }
                });

                Logger.LogInformation("User {UserId} successfully retrieved {MonitorCount} ban file monitor statuses",
                    User.XtremeIdiotsId(), models.Count);

                return View(models);
            }, "BanFileStatus");
        }

        /// <summary>
        /// Helper method to retrieve game server data for a ban file monitor
        /// </summary>
        /// <param name="gameServerId">The game server ID</param>
        /// <param name="banFileMonitorId">The ban file monitor ID for context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing action result (if error) and game server data</returns>
        private async Task<(IActionResult? ActionResult, GameServerDto? Data)> GetGameServerDataAsync(
            Guid gameServerId,
            Guid banFileMonitorId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(gameServerId, cancellationToken);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Game server {GameServerId} not found for ban file monitor {BanFileMonitorId}",
                        gameServerId, banFileMonitorId);
                    return (null, null);
                }

                return (null, gameServerApiResponse.Result.Data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving game server {GameServerId} for ban file monitor {BanFileMonitorId}",
                    gameServerId, banFileMonitorId);

                TrackErrorTelemetry(ex, "GetGameServerData", new Dictionary<string, string>
                {
                    { "GameServerId", gameServerId.ToString() },
                    { "BanFileMonitorId", banFileMonitorId.ToString() }
                });

                return (null, null);
            }
        }
    }
}