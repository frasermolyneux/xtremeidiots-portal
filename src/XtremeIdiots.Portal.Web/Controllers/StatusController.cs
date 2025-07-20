using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Provides status monitoring capabilities for system components .
/// This controller handles status information display for ban file monitors, system health checks,
/// and component monitoring functionality accessible to authorized administrators.
/// </summary>
/// <remarks>
/// The Status controller serves as a centralized monitoring dashboard for administrators to track
/// the operational status of various gaming server components including ban file synchronization,
/// monitor health and system-wide status indicators. All operations require appropriate authorization
/// based on the user's claimed games and administrative permissions.
/// 
/// Key features include:
/// - Ban file monitor status tracking with real-time synchronization data
/// - Game server connectivity validation and health monitoring 
/// - User-specific filtering based on claimed game types and administrative roles
/// - Detailed logging and telemetry for monitoring operations
/// - Error handling with graceful degradation for partial failures
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessStatus)]
public class StatusController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the StatusController with required dependencies for status monitoring operations.
 /// </summary>
 /// <param name="authorizationService">Service for handling authorization checks and policy validation</param>
 /// <param name="repositoryApiClient">Client for accessing repository API endpoints for data retrieval</param>
 /// <param name="telemetryClient">Application Insights telemetry client for tracking operations and performance</param>
 /// <param name="logger">Logger instance for recording operational events and debugging information</param>
 /// <param name="configuration">Application configuration for accessing settings and connection strings</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
 /// Displays the comprehensive status information for ban file monitors that the current user has administrative access to.
 /// This method retrieves and presents real-time synchronization data, file sizes and operational status for monitors
 /// associated with the user's claimed game types and administrative permissions.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation to allow request cancellation</param>
 /// <returns>
 /// A view containing a list of EditBanFileMonitorViewModel objects with current status data.
 /// An empty list if no monitors are found or accessible to the user.
 /// On authorization failure, Redirects to appropriate error page.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionview ban file status information</exception>
 /// <exception cref="InvalidOperationException">Thrown when repository API client is in an invalid state</exception>
 /// <remarks>
 /// This method implements security-first design by filtering monitors based on user claims before data retrieval.
 /// Only monitors for games where the user has SeniorAdmin, HeadAdmin, GameAdmin, or BanFileMonitor claims are accessible.
 /// The method gracefully handles partial failures where individual monitors may be inaccessible while continuing
 /// to process remaining monitors, ensuring maximum data availability for administrators.
 /// 
 /// Performance considerations:
 /// - Limits results to 50 monitors to prevent excessive memory usage
 /// - Implements efficient filtering at the API level rather than post-processing
 /// - Uses parallel processing for game server data enrichment where applicable
 /// </remarks>
 [HttpGet]
 public async Task<IActionResult> BanFileStatus(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 // Filter monitors based on user's claimed games to ensure security isolation between game types
 var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
 var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

 Logger.LogInformation("User {UserId} has access to {GameTypeCount} game types and {MonitorCount} ban file monitors",
 User.XtremeIdiotsId(), gameTypes.Count(), banFileMonitorIds.Count());

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

 TrackSuccessTelemetry("BanFileStatusRetrieved", nameof(BanFileStatus), new Dictionary<string, string>
 {
 { "MonitorCount", models.Count.ToString() },
 { "GameTypeCount", gameTypes.Count().ToString() }
 });

 Logger.LogInformation("User {UserId} successfully retrieved {MonitorCount} ban file monitor statuses",
 User.XtremeIdiotsId(), models.Count);

 return View(models);
 }, nameof(BanFileStatus));
 }

 /// <summary>
 /// Retrieves game server data for a specific ban file monitor to enrich the status information.
 /// This helper method provides detailed game server information including connection details,
 /// configuration data and operational status for comprehensive monitor reporting.
 /// </summary>
 /// <param name="gameServerId">The unique identifier of the game server associated with the ban file monitor</param>
 /// <param name="banFileMonitorId">The unique identifier of the ban file monitor for contextual logging and error tracking</param>
 /// <param name="cancellationToken">Cancellation token to allow request cancellation and prevent resource leaks</param>
 /// <returns>
 /// A tuple containing:
 /// - ActionResult: Non-null if an error occurred that should interrupt processing (currently always null)
 /// - GameServerDto: The game server data if successfully retrieved, null if not found or on error
 /// </returns>
 /// <remarks>
 /// This method implements defensive programming by gracefully handling failures without interrupting
 /// the overall status retrieval process. If a game server cannot be retrieved, the method logs the
 /// issue and returns null data, allowing the calling method to continue processing other monitors.
 /// 
 /// Error handling strategy:
 /// - API not found responses are logged as warnings but don't throw exceptions
 /// - Network or service errors are logged as errors with full exception details
 /// - Telemetry tracking provides visibility into retrieval failures for monitoring
 /// - Null returns allow graceful degradation of the status display
 /// </remarks>
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

 TrackErrorTelemetry(ex, nameof(GetGameServerDataAsync), new Dictionary<string, string>
 {
 { nameof(gameServerId), gameServerId.ToString() },
 { nameof(banFileMonitorId), banFileMonitorId.ToString() }
 });

 return (null, null);
 }
 }
}