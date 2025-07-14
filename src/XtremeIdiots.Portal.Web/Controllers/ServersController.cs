using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing public server information and server map displays
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessServers)]
    public class ServersController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<ServersController> logger;

        public ServersController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<ServersController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the list of enabled game servers for the portal
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The server list view with available game servers</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing servers list", User.XtremeIdiotsId());

                var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                    null, null, GameServerFilter.PortalServerListEnabled, 0, 50,
                    GameServerOrder.BannerServerListPosition, cancellationToken);

                if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve game servers for user {UserId}. API Success: {IsSuccess}",
                        User.XtremeIdiotsId(), gameServersApiResponse.IsSuccess);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var result = gameServersApiResponse.Result.Data.Items
                    .Select(gs => new ServersGameServerViewModel(gs))
                    .ToList();

                logger.LogInformation("User {UserId} successfully retrieved {ServerCount} servers",
                    User.XtremeIdiotsId(), result.Count);

                return View(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving servers list for user {UserId}", User.XtremeIdiotsId());

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("UserId", User.XtremeIdiotsId());
                exceptionTelemetry.Properties.TryAdd("Action", "Index");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the global map view showing recent player locations
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The map view with recent player geo-location data</returns>
        [HttpGet]
        public async Task<IActionResult> Map(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing servers map view", User.XtremeIdiotsId());

                var response = await repositoryApiClient.RecentPlayers.V1.GetRecentPlayers(
                    null, null, DateTime.UtcNow.AddHours(-48), RecentPlayersFilter.GeoLocated,
                    0, 200, null, cancellationToken);

                if (response.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve recent players for map view for user {UserId}. API Success: {IsSuccess}",
                        User.XtremeIdiotsId(), response.IsSuccess);
                    return View(new List<object>());
                }

                logger.LogInformation("User {UserId} successfully retrieved {PlayerCount} recent players for map view",
                    User.XtremeIdiotsId(), response.Result.Data.Items.Count());

                return View(response.Result.Data.Items);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving servers map view for user {UserId}", User.XtremeIdiotsId());

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("UserId", User.XtremeIdiotsId());
                exceptionTelemetry.Properties.TryAdd("Action", "Map");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays detailed information for a specific game server including maps, statistics, and timeline
        /// </summary>
        /// <param name="id">The unique identifier of the game server</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The server info view with comprehensive server data, or NotFound if server doesn't exist</returns>
        [HttpGet]
        public async Task<IActionResult> ServerInfo(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing server info for server {ServerId}",
                    User.XtremeIdiotsId(), id);

                var gameServerApiResponse = await repositoryApiClient.GameServers.V1.GetGameServer(id, cancellationToken);

                if (gameServerApiResponse.IsNotFound || gameServerApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Server {ServerId} not found when accessing server info for user {UserId}",
                        id, User.XtremeIdiotsId());
                    return NotFound();
                }

                var gameServerData = gameServerApiResponse.Result.Data;

                // Get current map information if available
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
                        logger.LogWarning(ex, "Failed to retrieve map {MapName} for server {ServerId}",
                            gameServerData.LiveMap, id);
                    }
                }

                // Get server statistics for the past 2 days
                var gameServerStatsResponseDto = await repositoryApiClient.GameServersStats.V1
                    .GetGameServerStatusStats(gameServerData.GameServerId, DateTime.UtcNow.AddDays(-2), cancellationToken);

                var mapTimelineDataPoints = new List<MapTimelineDataPoint>();
                var gameServerStatDtos = new List<GameServerStatDto>();
                var maps = new List<MapDto>();

                if (gameServerStatsResponseDto.IsSuccess && gameServerStatsResponseDto.Result?.Data?.Items != null)
                {
                    gameServerStatDtos = gameServerStatsResponseDto.Result.Data.Items.ToList();

                    // Build map timeline from statistics
                    GameServerStatDto? current = null;
                    var orderedStats = gameServerStatsResponseDto.Result.Data.Items.OrderBy(gss => gss.Timestamp).ToList();

                    foreach (var gameServerStatusStatDto in orderedStats)
                    {
                        if (current == null)
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

                    // Get map details for all maps played
                    try
                    {
                        var mapNames = gameServerStatsResponseDto.Result.Data.Items
                            .GroupBy(m => m.MapName)
                            .Select(m => m.Key)
                            .ToArray();

                        var mapsCollectionApiResponse = await repositoryApiClient.Maps.V1.GetMaps(
                            gameServerData.GameType, mapNames, null, null, 0, 50,
                            MapsOrder.MapNameAsc, cancellationToken);

                        if (mapsCollectionApiResponse.Result?.Data?.Items != null)
                            maps = mapsCollectionApiResponse.Result.Data.Items.ToList();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to retrieve map details for server {ServerId}", id);
                    }
                }

                var viewModel = new ServersGameServerViewModel(gameServerData)
                {
                    Map = mapDto,
                    Maps = maps,
                    GameServerStats = gameServerStatDtos,
                    MapTimelineDataPoints = mapTimelineDataPoints
                };

                logger.LogInformation("User {UserId} successfully retrieved server info for server {ServerId} with {MapCount} maps and {StatCount} statistics",
                    User.XtremeIdiotsId(), id, maps.Count, gameServerStatDtos.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving server info for server {ServerId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("UserId", User.XtremeIdiotsId());
                exceptionTelemetry.Properties.TryAdd("ServerId", id.ToString());
                exceptionTelemetry.Properties.TryAdd("Action", "ServerInfo");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }
    }
}