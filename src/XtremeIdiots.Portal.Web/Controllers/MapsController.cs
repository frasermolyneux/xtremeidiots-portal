using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing game maps display and functionality for Call of Duty servers.
/// Provides map browsing, filtering and image retrieval capabilities with server-side DataTable processing.
/// </summary>
/// <remarks>
/// This controller handles map-related operations including listing maps by game type,
/// providing data for DataTable Ajax requests with server-side processing and serving map images.
/// All operations are secured with the AccessMaps authorization policy.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessMaps)]
public class MapsController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the <see cref="MapsController"/> class.
 /// </summary>
 /// <param name="authorizationService">Service for handling authorization policies and requirements</param>
 /// <param name="repositoryApiClient">Client for accessing the Repository API for map data</param>
 /// <param name="telemetryClient">Application Insights telemetry client for tracking operations</param>
 /// <param name="logger">Logger instance for this controller</param>
 /// <param name="configuration">Application configuration provider</param>
 /// <exception cref="ArgumentNullException">Thrown when any required parameter is null</exception>
 public MapsController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<MapsController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 }

 /// <summary>
 /// Displays the maps index page showing all available maps across all game types.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The maps index view with client-side DataTable processing</returns>
 /// <remarks>
 /// This action serves as the main entry point for the maps section, displaying a DataTable
 /// that will be populated via Ajax calls to <see cref="GetMapListAjax"/>.
 /// </remarks>
 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(() =>
 {
 return Task.FromResult<IActionResult>(View());
 }, nameof(Index));
 }

 /// <summary>
 /// Displays the maps index page filtered by a specific game type.
 /// </summary>
 /// <param name="id">The game type to filter maps by. If null, shows all maps</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The maps index view filtered by the specified game type</returns>
 /// <remarks>
 /// This action reuses the Index view but sets ViewData["GameType"] to filter the DataTable
 /// to show only maps for the specified Call of Duty game variant.
 /// </remarks>
 [HttpGet]
 public async Task<IActionResult> GameIndex(GameType? id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(() =>
 {
 ViewData["GameType"] = id;
 return Task.FromResult<IActionResult>(View(nameof(Index)));
 }, nameof(GameIndex));
 }

 /// <summary>
 /// Retrieves map data for DataTable Ajax requests with server-side processing, filtering and sorting.
 /// </summary>
 /// <param name="id">Optional game type to filter maps by. If null, returns maps for all game types</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>JSON data formatted for DataTable consumption with pagination and search results</returns>
 /// <remarks>
 /// This endpoint handles server-side DataTable processing including:
 /// - Filtering by search terms across map names
 /// - Sorting by map name, popularity, or game type
 /// - Pagination with configurable page sizes
 /// - Game type filtering for Call of Duty variants (COD2, COD4, COD5)
 /// The request body contains DataTable Ajax parameters in JSON format.
 /// </remarks>
 /// <exception cref="BadRequestResult">Thrown when the request body cannot be deserialized</exception>
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> GetMapListAjax(GameType? id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var reader = new StreamReader(Request.Body);
 var requestBody = await reader.ReadToEndAsync();

 var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

 if (model is null)
 {
 Logger.LogWarning("Invalid DataTable request body for user {UserId}", User.XtremeIdiotsId());
 return BadRequest();
 }

 var order = MapsOrder.MapNameAsc;

 var orderColumn = model.Columns[model.Order.First().Column].Name;
 var searchOrder = model.Order.First().Dir;

 switch (orderColumn)
 {
 case "mapName":
 order = searchOrder == "asc" ? MapsOrder.MapNameAsc : MapsOrder.MapNameDesc;
 break;
 case "popularity":
 order = searchOrder == "asc" ? MapsOrder.PopularityAsc : MapsOrder.PopularityDesc;
 break;
 case "gameType":
 order = searchOrder == "asc" ? MapsOrder.GameTypeAsc : MapsOrder.GameTypeDesc;
 break;
 }

 var mapsApiResponse = await repositoryApiClient.Maps.V1.GetMaps(id, null, null, model.Search?.Value, model.Start, model.Length, order);

 if (!mapsApiResponse.IsSuccess || mapsApiResponse.Result?.Data is null)
 {
 Logger.LogWarning("Failed to retrieve maps data for user {UserId} and game type {GameType}",
 User.XtremeIdiotsId(), id);
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController)[..^10], new { id = 500 });
 }

 TrackSuccessTelemetry("MapsListRetrieved", nameof(GetMapListAjax), new Dictionary<string, string>
 {
 { "GameType", id?.ToString() ?? "All" },
 { "ResultCount", mapsApiResponse.Result.Data.Items?.Count().ToString() ?? "0" }
 });

 return Json(new
 {
 model.Draw,
 recordsTotal = mapsApiResponse.Result.Data.TotalCount,
 recordsFiltered = mapsApiResponse.Result.Data.FilteredCount,
 data = mapsApiResponse.Result.Data.Items
 });
 }, nameof(GetMapListAjax));
 }

 /// <summary>
 /// Retrieves the map image for a specific game type and map name.
 /// </summary>
 /// <param name="gameType">The game type for the map (COD2, COD4, COD5, etc.)</param>
 /// <param name="mapName">The name of the map to retrieve the image for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Redirect to the map image URL or default no-image placeholder</returns>
 /// <remarks>
 /// This endpoint serves map preview images for the maps listing. If no image is available
 /// for the specified map, it Redirects to a default "no image" placeholder.
 /// Images are served via redirect to external storage URLs rather than proxying the content.
 /// </remarks>
 /// <exception cref="BadRequestResult">Thrown when gameType is Unknown or mapName is null/whitespace</exception>
 [HttpGet]
 public async Task<IActionResult> MapImage(GameType gameType, string mapName, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(mapName))
 {
 Logger.LogWarning("Invalid map image request with game type {GameType} and map name {MapName} from user {UserId}",
 gameType, mapName ?? "null", User.XtremeIdiotsId());
 return BadRequest();
 }

 var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(gameType, mapName);

 if (!mapApiResponse.IsSuccess || mapApiResponse.Result?.Data is null || string.IsNullOrWhiteSpace(mapApiResponse.Result.Data.MapImageUri))
 {
 Logger.LogWarning("Map image not found for {GameType} map {MapName} requested by user {UserId}",
 gameType, mapName, User.XtremeIdiotsId());
 return Redirect("/images/noimage.jpg");
 }

 TrackSuccessTelemetry("MapImageRetrieved", "MapImage", new Dictionary<string, string>
 {
 { "GameType", gameType.ToString() },
 { "MapName", mapName }
 });

 return Redirect(mapApiResponse.Result.Data.MapImageUri);
 }, nameof(MapImage));
 }
}