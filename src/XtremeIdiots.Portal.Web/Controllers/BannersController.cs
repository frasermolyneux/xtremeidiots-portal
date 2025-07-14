using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using MX.Api.Abstractions;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameTracker;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing game server banners and GameTracker integration
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessHome)]
    public class BannersController : Controller
    {
        private const string GameServersListCacheKey = "game-servers-api-response";

        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IMemoryCache memoryCache;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<BannersController> logger;

        /// <summary>
        /// Initializes a new instance of the BannersController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="repositoryApiClient">Client for accessing repository API endpoints</param>
        /// <param name="memoryCache">Memory cache for storing temporary data</param>
        /// <param name="telemetryClient">Client for tracking telemetry and events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public BannersController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IMemoryCache memoryCache,
            TelemetryClient telemetryClient,
            ILogger<BannersController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the game servers list view for banner management
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The game servers list view or an authorization failure response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to access game servers</exception>
        [HttpGet]
        public async Task<IActionResult> GameServersList(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to access game servers list view", User.XtremeIdiotsId());

                var canAccessGameServers = await authorizationService.AuthorizeAsync(User, AuthPolicies.AccessHome);
                if (!canAccessGameServers.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to game servers list view", User.XtremeIdiotsId());

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Banners");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GameServersList");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServersList");
                    unauthorizedTelemetry.Properties.TryAdd("Context", "BannerManagement");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("User {UserId} successfully accessed game servers list view", User.XtremeIdiotsId());

                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading game servers list view for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("Action", "GameServersList");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Gets the list of game servers with banner information (CORS enabled for external access)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON array of HTML banners or error response</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to access game servers</exception>
        [HttpGet]
        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetGameServers(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to retrieve game servers banners data", User.XtremeIdiotsId());

                var canAccessGameServers = await authorizationService.AuthorizeAsync(User, AuthPolicies.AccessHome);
                if (!canAccessGameServers.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to game servers banners data", User.XtremeIdiotsId());

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Banners");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GetGameServers");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameServersBanners");
                    unauthorizedTelemetry.Properties.TryAdd("Context", "BannerData");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                ApiResult<CollectionModel<GameServerDto>>? gameServersApiResponse;

                if (memoryCache.TryGetValue(GameServersListCacheKey, out gameServersApiResponse) && gameServersApiResponse != null)
                {
                    logger.LogDebug("Retrieved game servers data from cache for user {UserId}", User.XtremeIdiotsId());
                }
                else
                {
                    logger.LogDebug("Fetching game servers data from API for user {UserId}", User.XtremeIdiotsId());

                    gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                        null, null, GameServerFilter.BannerServerListEnabled, 0, 50,
                        GameServerOrder.BannerServerListPosition, cancellationToken);

                    if (gameServersApiResponse != null)
                    {
                        memoryCache.Set(GameServersListCacheKey, gameServersApiResponse, DateTime.UtcNow.AddMinutes(5));
                        logger.LogDebug("Cached game servers data for user {UserId}", User.XtremeIdiotsId());
                    }
                }

                if (gameServersApiResponse?.IsSuccess != true || gameServersApiResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve game servers data or data is null for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var htmlBanners = gameServersApiResponse.Result.Data.Items
                    .Where(gs => !string.IsNullOrEmpty(gs.HtmlBanner))
                    .Select(gs => gs.HtmlBanner)
                    .ToList();

                logger.LogInformation("User {UserId} successfully retrieved {BannerCount} HTML banners",
                    User.XtremeIdiotsId(), htmlBanners.Count);

                return Json(htmlBanners);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving game servers banners data for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("Action", "GetGameServers");
                telemetryClient.TrackException(errorTelemetry);

                return RedirectToAction("Display", "Errors", new { id = 500 });
            }
        }

        /// <summary>
        /// Redirects to GameTracker banner with caching support and fallback handling
        /// </summary>
        /// <param name="ipAddress">Server IP address for the GameTracker banner</param>
        /// <param name="queryPort">Server query port for the GameTracker banner</param>
        /// <param name="imageName">Banner image name requested from GameTracker</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to banner URL or fallback GameTracker URL</returns>
        /// <exception cref="ArgumentException">Thrown when invalid parameters are provided</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to access GameTracker banners</exception>
        [HttpGet]
        [Route("gametracker/{ipAddress}:{queryPort}/{imageName}")]
        public async Task<IActionResult> GetGameTrackerBanner(string ipAddress, string queryPort, string imageName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(queryPort) || string.IsNullOrWhiteSpace(imageName))
                {
                    logger.LogWarning("User {UserId} provided invalid parameters for GameTracker banner request: IP={IpAddress}, Port={QueryPort}, Image={ImageName}",
                        User.XtremeIdiotsId(), ipAddress, queryPort, imageName);

                    return BadRequest("Invalid parameters provided");
                }

                logger.LogInformation("User {UserId} attempting to retrieve GameTracker banner for {IpAddress}:{QueryPort}/{ImageName}",
                    User.XtremeIdiotsId(), ipAddress, queryPort, imageName);

                var canAccessGameTracker = await authorizationService.AuthorizeAsync(User, AuthPolicies.AccessHome);
                if (!canAccessGameTracker.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to GameTracker banner for {IpAddress}:{QueryPort}/{ImageName}",
                        User.XtremeIdiotsId(), ipAddress, queryPort, imageName);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Banners");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "GetGameTrackerBanner");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "GameTrackerBanner");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"IpAddress:{ipAddress},QueryPort:{queryPort},ImageName:{imageName}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var cacheKey = $"{ipAddress}_{queryPort}_{imageName}";
                ApiResult<GameTrackerBannerDto>? repositoryApiResponse;

                if (memoryCache.TryGetValue(cacheKey, out repositoryApiResponse) && repositoryApiResponse != null)
                {
                    logger.LogDebug("Retrieved GameTracker banner data from cache for {IpAddress}:{QueryPort}/{ImageName}",
                        ipAddress, queryPort, imageName);
                }
                else
                {
                    logger.LogDebug("Fetching GameTracker banner data from API for {IpAddress}:{QueryPort}/{ImageName}",
                        ipAddress, queryPort, imageName);

                    repositoryApiResponse = await repositoryApiClient.GameTrackerBanner.V1.GetGameTrackerBanner(
                        ipAddress, queryPort, imageName, cancellationToken);

                    if (repositoryApiResponse != null)
                    {
                        memoryCache.Set(cacheKey, repositoryApiResponse, DateTime.UtcNow.AddMinutes(30));
                        logger.LogDebug("Cached GameTracker banner data for {IpAddress}:{QueryPort}/{ImageName}",
                            ipAddress, queryPort, imageName);
                    }
                }

                if (repositoryApiResponse?.IsSuccess != true || repositoryApiResponse.Result?.Data?.BannerUrl == null)
                {
                    var fallbackUrl = $"https://cache.gametracker.com/server_info/{ipAddress}:{queryPort}/{imageName}";

                    logger.LogWarning("GameTracker banner API failed or returned null data for {IpAddress}:{QueryPort}/{ImageName}, redirecting to fallback: {FallbackUrl}",
                        ipAddress, queryPort, imageName, fallbackUrl);

                    return Redirect(fallbackUrl);
                }

                var bannerUrl = repositoryApiResponse.Result.Data.BannerUrl;

                logger.LogInformation("User {UserId} successfully retrieved GameTracker banner URL for {IpAddress}:{QueryPort}/{ImageName}: {BannerUrl}",
                    User.XtremeIdiotsId(), ipAddress, queryPort, imageName, bannerUrl);

                return Redirect(bannerUrl);
            }
            catch (Exception ex)
            {
                var fallbackUrl = $"https://cache.gametracker.com/server_info/{ipAddress}:{queryPort}/{imageName}";

                logger.LogError(ex, "Error retrieving GameTracker banner for user {UserId} and {IpAddress}:{QueryPort}/{ImageName}, redirecting to fallback: {FallbackUrl}",
                    User.XtremeIdiotsId(), ipAddress, queryPort, imageName, fallbackUrl);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("IpAddress", ipAddress ?? string.Empty);
                errorTelemetry.Properties.TryAdd("QueryPort", queryPort ?? string.Empty);
                errorTelemetry.Properties.TryAdd("ImageName", imageName ?? string.Empty);
                errorTelemetry.Properties.TryAdd("Action", "GetGameTrackerBanner");
                errorTelemetry.Properties.TryAdd("FallbackUrl", fallbackUrl);
                telemetryClient.TrackException(errorTelemetry);

                return Redirect(fallbackUrl);
            }
        }
    }
}