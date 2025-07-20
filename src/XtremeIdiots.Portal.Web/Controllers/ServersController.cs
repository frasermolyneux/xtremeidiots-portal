using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing server information and functionality
/// </summary>
[Authorize(Policy = AuthPolicies.AccessServers)]
public class ServersController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IRepositoryApiClient repositoryApiClient;

    /// <summary>
    /// Initializes a new instance of the ServersController
    /// </summary>
    /// <param name="authorizationService">Service for handling authorization checks</param>
    /// <param name="repositoryApiClient">Client for repository API operations</param>
    /// <param name="telemetryClient">Client for application telemetry</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
    public ServersController(
        IAuthorizationService authorizationService,
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<ServersController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    /// <summary>
    /// Displays the main servers listing page
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with list of enabled servers or error page on failure</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                null, null, GameServerFilter.PortalServerListEnabled, 0, 50,
                GameServerOrder.BannerServerListPosition, cancellationToken);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve game servers for user {UserId}. API Success: {IsSuccess}",
                    User.XtremeIdiotsId(), gameServersApiResponse.IsSuccess);
                return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController).Replace("Controller", ""), new { id = 500 });
            }

            var result = gameServersApiResponse.Result.Data.Items
                .Select(gs => new ServersGameServerViewModel(gs))
                .ToList();

            Logger.LogInformation("User {UserId} successfully retrieved {ServerCount} servers",
                User.XtremeIdiotsId(), result.Count);

            return View(result);
        }, nameof(Index));
    }

    /// <summary>
    /// Displays the interactive map view showing recent player locations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with geo-located recent players or empty list on failure</returns>
    [HttpGet]
    public async Task<IActionResult> Map(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var response = await repositoryApiClient.RecentPlayers.V1.GetRecentPlayers(
                null, null, DateTime.UtcNow.AddHours(-48), RecentPlayersFilter.GeoLocated,
                0, 200, null, cancellationToken);

            if (response.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve recent players for map view for user {UserId}. API Success: {IsSuccess}",
                    User.XtremeIdiotsId(), response.IsSuccess);
                return View(new List<object>());
            }

            Logger.LogInformation("User {UserId} successfully retrieved {PlayerCount} recent players for map view",
                User.XtremeIdiotsId(), response.Result.Data.Items.Count());

            return View(response.Result.Data.Items);
        }, nameof(Map));
    }

    /// <summary>
    /// Displays detailed information for a specific game server including statistics and map timeline
    /// </summary>
    /// <param name="id">The unique identifier of the game server</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Server details view with maps, statistics, and timeline data or NotFound if server doesn't exist</returns>
    [HttpGet]
    public async Task<IActionResult> ServerInfo(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

            if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data is null)
            {
                Logger.LogWarning("Server {ServerId} not found when accessing server info for user {UserId}",
                    id, User.XtremeIdiotsId());
                return NotFound();
            }

            var gameServerData = gameServerApiResponse.Result.Data;

            MapDto? mapDto = null;
            if (!string.IsNullOrWhiteSpace(gameServerData.LiveMap))
            {
                try
                {
                    var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(
                        gameServerData.GameType, gameServerData.LiveMap, cancellationToken);
                    mapDto = mapApiResponse.Result?.Data;
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to retrieve map {MapName} for server {ServerId}",
                        gameServerData.LiveMap, id);
                }
            }

            var gameServerStatsResponseDto = await repositoryApiClient.GameServersStats.V1
                .GetGameServerStatusStats(gameServerData.GameServerId, DateTime.UtcNow.AddDays(-2), cancellationToken);

            var mapTimelineDataPoints = new List<MapTimelineDataPoint>();
            var gameServerStatDtos = new List<GameServerStatDto>();
            var maps = new List<MapDto>();

            if (gameServerStatsResponseDto.IsSuccess && gameServerStatsResponseDto.Result?.Data?.Items is not null)
            {
                gameServerStatDtos = gameServerStatsResponseDto.Result.Data.Items.ToList();

                GameServerStatDto? current = null;
                var orderedStats = gameServerStatsResponseDto.Result.Data.Items.OrderBy(gss => gss.Timestamp).ToList();

                foreach (var gameServerStatusStatDto in orderedStats)
                {
                    if (current is null)
                    {
                        current = gameServerStatusStatDto;
                        continue;
                    }

                    if (current.MapName != gameServerStatusStatDto.MapName)
                    {
                        mapTimelineDataPoints.Add(new MapTimelineDataPoint(
                            current.MapName, current.Timestamp, gameServerStatusStatDto.Timestamp));
                        current = gameServerStatusStatDto;
                        continue;
                    }

                    if (current == orderedStats.Last())
                        mapTimelineDataPoints.Add(new MapTimelineDataPoint(
                            current.MapName, current.Timestamp, DateTime.UtcNow));
                }

                try
                {
                    var mapNames = gameServerStatsResponseDto.Result.Data.Items
                        .GroupBy(m => m.MapName)
                        .Select(m => m.Key)
                        .ToArray();

                    var mapsCollectionApiResponse = await repositoryApiClient.Maps.V1.GetMaps(
                        gameServerData.GameType, mapNames, null, null, 0, 50,
                        MapsOrder.MapNameAsc, cancellationToken);

                    if (mapsCollectionApiResponse.Result?.Data?.Items is not null)
                        maps = mapsCollectionApiResponse.Result.Data.Items.ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to retrieve map details for server {ServerId}", id);
                }
            }

            var viewModel = new ServersGameServerViewModel(gameServerData)
            {
                Map = mapDto,
                Maps = maps,
                GameServerStats = gameServerStatDtos,
                MapTimelineDataPoints = mapTimelineDataPoints
            };

            Logger.LogInformation("User {UserId} successfully retrieved server info for server {ServerId} with {MapCount} maps and {StatCount} statistics",
                User.XtremeIdiotsId(), id, maps.Count, gameServerStatDtos.Count);

            return View(viewModel);
        }, nameof(ServerInfo));
    }
}