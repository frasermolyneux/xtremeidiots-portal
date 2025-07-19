using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing game maps display and functionality
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessMaps)]
    public class MapsController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

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
        /// Displays the maps index page showing all available maps
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The maps index view</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(() =>
            {
                return Task.FromResult<IActionResult>(View());
            }, "Index");
        }

        /// <summary>
        /// Displays the maps index page filtered by a specific game type
        /// </summary>
        /// <param name="id">The game type to filter maps by</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The maps index view filtered by game type</returns>
        [HttpGet]
        public async Task<IActionResult> GameIndex(GameType? id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(() =>
            {
                ViewData["GameType"] = id;
                return Task.FromResult<IActionResult>(View(nameof(Index)));
            }, "GameIndex");
        }

        /// <summary>
        /// Retrieves map data for DataTable Ajax requests with server-side processing
        /// </summary>
        /// <param name="id">Optional game type to filter maps by</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data formatted for DataTable consumption</returns>
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
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                TrackSuccessTelemetry("MapsListRetrieved", "GetMapListAjax", new Dictionary<string, string>
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
            }, "GetMapListAjax");
        }

        /// <summary>
        /// Retrieves the map image for a specific game type and map name
        /// </summary>
        /// <param name="gameType">The game type for the map</param>
        /// <param name="mapName">The name of the map</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to the map image URL or default no-image placeholder</returns>
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
            }, "MapImage");
        }
    }
}