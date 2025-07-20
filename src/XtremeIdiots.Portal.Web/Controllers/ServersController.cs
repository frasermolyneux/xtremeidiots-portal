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
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Maps;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for displaying public server information including real-time statistics
/// and map rotation data. Provides the community-facing view of server status
/// without exposing administrative functions.
/// </summary>
/// <remarks>
/// This controller handles the public-facing server display functionality .
/// It provides read-only access to server information, real-time statistics, map rotations and geographical
/// player distribution data. The controller supports community members viewing server status, map timelines,
/// and recent player activity without requiring administrative privileges.
/// 
/// Key features include:
/// - Server list display with live statistics
/// - Global map view showing recent player locations
/// - Detailed server information with map rotation history
/// - Integration with Repository API for game server data
/// - Geolocation visualization for community engagement
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessServers)]
public class ServersController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the <see cref="ServersController"/> class.
 /// </summary>
 /// <param name="authorizationService">Service for handling authorization policies and requirements</param>
 /// <param name="repositoryApiClient">Client for accessing the repository API to retrieve server and player data</param>
 /// <param name="telemetryClient">Application Insights telemetry client for tracking server access events</param>
 /// <param name="logger">Logger instance for recording server display operations and errors</param>
 /// <param name="configuration">Configuration settings for server display behavior and API endpoints</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
 /// Displays the list of enabled game servers for the portal
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The server list view with available game servers</returns>
 /// <exception cref="InvalidOperationException">Thrown when the Repository API fails to return server data</exception>
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
 /// Displays the global map view showing recent player locations
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The map view with recent player geo-location data</returns>
 /// <exception cref="InvalidOperationException">Thrown when the Repository API fails to return recent player data</exception>
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
 /// Displays detailed information for a specific game server including maps, statistics and timeline
 /// </summary>
 /// <param name="id">The unique identifier of the game server</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The server info view with comprehensive server data, or NotFound if server doesn't exist</returns>
 /// <exception cref="InvalidOperationException">Thrown when the Repository API fails to return server data</exception>
 /// <exception cref="ArgumentException">Thrown when the server ID is invalid or malformed</exception>
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

 // Fetch current map details to display additional context about the game state
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

 // Retrieve recent server statistics to build a map rotation timeline
 // This helps players understand server activity patterns and map preferences
 var gameServerStatsResponseDto = await repositoryApiClient.GameServersStats.V1
 .GetGameServerStatusStats(gameServerData.GameServerId, DateTime.UtcNow.AddDays(-2), cancellationToken);

 var mapTimelineDataPoints = new List<MapTimelineDataPoint>();
 var gameServerStatDtos = new List<GameServerStatDto>();
 var maps = new List<MapDto>();

 if (gameServerStatsResponseDto.IsSuccess && gameServerStatsResponseDto.Result?.Data?.Items != null)
 {
 gameServerStatDtos = gameServerStatsResponseDto.Result.Data.Items.ToList();

 // Build timeline showing when map changes occurred
 // This algorithm identifies map transitions by comparing consecutive statistics
 GameServerStatDto? current = null;
 var orderedStats = gameServerStatsResponseDto.Result.Data.Items.OrderBy(gss => gss.Timestamp).ToList();

 foreach (var gameServerStatusStatDto in orderedStats)
 {
 if (current is null)
 {
 current = gameServerStatusStatDto;
 continue;
 }

 // Map change detected - record the previous map's duration
 if (current.MapName != gameServerStatusStatDto.MapName)
 {
 mapTimelineDataPoints.Add(new MapTimelineDataPoint(
 current.MapName, current.Timestamp, gameServerStatusStatDto.Timestamp));
 current = gameServerStatusStatDto;
 continue;
 }

 // Handle the final map in the timeline
 if (current == orderedStats.Last())
 mapTimelineDataPoints.Add(new MapTimelineDataPoint(
 current.MapName, current.Timestamp, DateTime.UtcNow));
 }

 // Enrich timeline with map metadata for better visual presentation
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