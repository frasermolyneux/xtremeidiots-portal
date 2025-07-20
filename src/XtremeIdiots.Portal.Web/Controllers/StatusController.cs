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

    [HttpGet]
    public async Task<IActionResult> BanFileStatus(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {

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
 { "GameTypeCount", gameTypes.Count().ToString() }
        });

            Logger.LogInformation("User {UserId} successfully retrieved {MonitorCount} ban file monitor statuses",
     User.XtremeIdiotsId(), models.Count);

            return View(models);
        }, nameof(BanFileStatus));
    }

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