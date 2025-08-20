using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing system status and monitoring operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the StatusController
/// </remarks>
/// <param name="repositoryApiClient">Client for repository API operations</param>
/// <param name="telemetryClient">Client for application telemetry</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
/// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
[Authorize(Policy = AuthPolicies.AccessStatus)]
public class StatusController(
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<StatusController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the ban file monitor status page showing synchronization status and file information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with ban file monitor status information or empty list if none found</returns>
    [HttpGet]
    public async Task<IActionResult> BanFileStatus(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin, UserProfileClaimType.GameAdmin, UserProfileClaimType.BanFileMonitor };
            var (gameTypes, banFileMonitorIds) = User.ClaimedGamesAndItems(requiredClaims);

            Logger.LogInformation("User {UserId} has access to {GameTypeCount} game types and {MonitorCount} ban file monitors",
                User.XtremeIdiotsId(), gameTypes.Length, banFileMonitorIds.Length);

            var banFileMonitorsApiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(
                gameTypes, banFileMonitorIds, null, 0, 50, BanFileMonitorOrder.BannerServerListPosition, cancellationToken);

            if (banFileMonitorsApiResponse.IsNotFound || banFileMonitorsApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("No ban file monitors found for user {UserId}", User.XtremeIdiotsId());
                return View(new List<EditBanFileMonitorViewModel>());
            }

            var models = new List<EditBanFileMonitorViewModel>();

            foreach (var banFileMonitor in banFileMonitorsApiResponse.Result.Data.Items)
            {
                var (actionResult, gameServerData) = await GetGameServerDataAsync(banFileMonitor.GameServerId, banFileMonitor.BanFileMonitorId, cancellationToken);

                if (actionResult is not null)
                {
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
                { "GameTypeCount", gameTypes.Length.ToString() }
            });

            Logger.LogInformation("User {UserId} successfully retrieved {MonitorCount} ban file monitor statuses",
                User.XtremeIdiotsId(), models.Count);

            return View(models);
        }, nameof(BanFileStatus));
    }

    /// <summary>
    /// Retrieves game server data for a specific ban file monitor
    /// </summary>
    /// <param name="gameServerId">The unique identifier of the game server</param>
    /// <param name="banFileMonitorId">The unique identifier of the ban file monitor</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Tuple containing potential action result for errors and game server data if successful</returns>
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