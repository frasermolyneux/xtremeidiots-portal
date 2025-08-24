using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Provides map browsing, search, and image retrieval functionality
/// </summary>
/// <remarks>
/// Initializes a new instance of the MapsController
/// </remarks>
/// <param name="authorizationService">Service for checking user authorization</param>
/// <param name="repositoryApiClient">Client for accessing repository data</param>
/// <param name="telemetryClient">Client for tracking telemetry data</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.AccessMaps)]
public class MapsController(
    IAuthorizationService authorizationService,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<MapsController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the main maps index page
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Maps index view</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(() => Task.FromResult<IActionResult>(View()), nameof(Index));
    }

    /// <summary>
    /// Displays the maps index page filtered by game type
    /// </summary>
    /// <param name="id">Game type to filter by</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Maps index view with game type filter applied</returns>
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
    /// Provides paginated, searchable map data for DataTables Ajax requests
    /// </summary>
    /// <param name="id">Optional game type to filter maps</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JSON data formatted for DataTables consumption</returns>
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
                default:
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
                recordsTotal = mapsApiResponse.Result?.Pagination?.TotalCount,
                recordsFiltered = mapsApiResponse.Result?.Pagination?.FilteredCount,
                data = mapsApiResponse?.Result?.Data?.Items
            });
        }, nameof(GetMapListAjax));
    }

    /// <summary>
    /// Retrieves and redirects to a map image or returns a default image if not found
    /// </summary>
    /// <param name="gameType">Game type of the map</param>
    /// <param name="mapName">Name of the map</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirect to map image URI or default no-image placeholder</returns>
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