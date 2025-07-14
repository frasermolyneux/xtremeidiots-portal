using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for displaying status information about system components
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessStatus)]
    public class StatusController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<StatusController> logger;

        public StatusController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<StatusController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                logger.LogInformation("User {UserId} accessing ban file status", User.XtremeIdiotsId());

                // Check specific authorization for viewing ban file status
                var authResult = await authorizationService.AuthorizeAsync(User, AuthPolicies.AccessStatus);
                if (!authResult.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to ban file status", User.XtremeIdiotsId());

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Status");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "BanFileStatus");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "BanFileStatus");
                    unauthorizedTelemetry.Properties.TryAdd("Context", "ViewBanFileMonitorStatus");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Forbid();
                }

                // Get user's claimed games and ban file monitor access
                var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
                var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

                logger.LogInformation("User {UserId} has access to {GameTypeCount} game types and {MonitorCount} ban file monitors",
                    User.XtremeIdiotsId(), gameTypes.Count(), banFileMonitorIds.Count());

                // Retrieve ban file monitors
                var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(
                    gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition, cancellationToken);

                if (banFileMonitorsApiResponse.IsNotFound || banFileMonitorsApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("No ban file monitors found for user {UserId}", User.XtremeIdiotsId());
                    return View(new List<EditBanFileMonitorViewModel>());
                }

                var models = new List<EditBanFileMonitorViewModel>();

                // Process each ban file monitor and enrich with game server data
                foreach (var banFileMonitor in banFileMonitorsApiResponse.Result.Data.Items)
                {
                    try
                    {
                        var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(banFileMonitor.GameServerId, cancellationToken);

                        if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                        {
                            logger.LogWarning("Game server {GameServerId} not found for ban file monitor {BanFileMonitorId}",
                                banFileMonitor.GameServerId, banFileMonitor.BanFileMonitorId);
                            continue;
                        }

                        models.Add(new EditBanFileMonitorViewModel
                        {
                            BanFileMonitorId = banFileMonitor.BanFileMonitorId,
                            FilePath = banFileMonitor.FilePath,
                            RemoteFileSize = banFileMonitor.RemoteFileSize,
                            LastSync = banFileMonitor.LastSync,
                            GameServerId = banFileMonitor.GameServerId,
                            GameServer = gameServerApiResponse.Result.Data
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing ban file monitor {BanFileMonitorId} for game server {GameServerId}",
                            banFileMonitor.BanFileMonitorId, banFileMonitor.GameServerId);

                        var exceptionTelemetry = new ExceptionTelemetry(ex)
                        {
                            SeverityLevel = SeverityLevel.Error
                        };
                        exceptionTelemetry.Properties.TryAdd("UserId", User.XtremeIdiotsId());
                        exceptionTelemetry.Properties.TryAdd("BanFileMonitorId", banFileMonitor.BanFileMonitorId.ToString());
                        exceptionTelemetry.Properties.TryAdd("GameServerId", banFileMonitor.GameServerId.ToString());
                        exceptionTelemetry.Properties.TryAdd("Action", "ProcessBanFileMonitor");
                        telemetryClient.TrackException(exceptionTelemetry);

                        // Continue processing other monitors instead of failing completely
                        continue;
                    }
                }

                logger.LogInformation("User {UserId} successfully retrieved {MonitorCount} ban file monitor statuses",
                    User.XtremeIdiotsId(), models.Count);

                return View(models);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving ban file status for user {UserId}", User.XtremeIdiotsId());

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("UserId", User.XtremeIdiotsId());
                exceptionTelemetry.Properties.TryAdd("Controller", "Status");
                exceptionTelemetry.Properties.TryAdd("Action", "BanFileStatus");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }
    }
}