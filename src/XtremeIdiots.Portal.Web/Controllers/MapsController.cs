using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class MapsController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<MapsController> logger;

        public MapsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<MapsController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the maps index page showing all available maps
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The maps index view</returns>
        [HttpGet]
        public IActionResult Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing maps index", User.XtremeIdiotsId());
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing maps index for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the maps index page filtered by a specific game type
        /// </summary>
        /// <param name="id">The game type to filter maps by</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The maps index view filtered by game type</returns>
        [HttpGet]
        public IActionResult GameIndex(GameType? id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing maps index for game type {GameType}",
                    User.XtremeIdiotsId(), id);

                ViewData["GameType"] = id;
                return View(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing maps index for game type {GameType} and user {UserId}",
                    id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameType", id?.ToString() ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} requesting map list data for game type {GameType}",
                    User.XtremeIdiotsId(), id);

                var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                var model = JsonConvert.DeserializeObject<DataTableAjaxPostModel>(requestBody);

                if (model == null)
                {
                    logger.LogWarning("Invalid DataTable request body for user {UserId}", User.XtremeIdiotsId());
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

                if (!mapsApiResponse.IsSuccess || mapsApiResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve maps data for user {UserId} and game type {GameType}",
                        User.XtremeIdiotsId(), id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved maps for user {UserId}", User.XtremeIdiotsId());

                return Json(new
                {
                    model.Draw,
                    recordsTotal = mapsApiResponse.Result.Data.TotalCount,
                    recordsFiltered = mapsApiResponse.Result.Data.FilteredCount,
                    data = mapsApiResponse.Result.Data.Items
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving map list for user {UserId} and game type {GameType}",
                    User.XtremeIdiotsId(), id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameType", id?.ToString() ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                if (gameType == GameType.Unknown || string.IsNullOrWhiteSpace(mapName))
                {
                    logger.LogWarning("Invalid map image request with game type {GameType} and map name {MapName} from user {UserId}",
                        gameType, mapName ?? "null", User.XtremeIdiotsId());
                    return BadRequest();
                }

                logger.LogInformation("User {UserId} requesting map image for {GameType} map {MapName}",
                    User.XtremeIdiotsId(), gameType, mapName);

                var mapApiResponse = await repositoryApiClient.Maps.V1.GetMap(gameType, mapName);

                if (!mapApiResponse.IsSuccess || mapApiResponse.Result?.Data == null || string.IsNullOrWhiteSpace(mapApiResponse.Result.Data.MapImageUri))
                {
                    logger.LogWarning("Map image not found for {GameType} map {MapName} requested by user {UserId}",
                        gameType, mapName, User.XtremeIdiotsId());
                    return Redirect("/images/noimage.jpg");
                }

                logger.LogInformation("Successfully retrieved map image URI for {GameType} map {MapName} for user {UserId}",
                    gameType, mapName, User.XtremeIdiotsId());

                return Redirect(mapApiResponse.Result.Data.MapImageUri);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving map image for {GameType} map {MapName} and user {UserId}",
                    gameType, mapName, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("GameType", gameType.ToString());
                errorTelemetry.Properties.TryAdd("MapName", mapName ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
    }
}