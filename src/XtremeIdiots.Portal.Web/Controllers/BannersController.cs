using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    public class BannersController : BaseController
    {
        private const string GameServersListCacheKey = "game-servers-api-response";

        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the BannersController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="repositoryApiClient">Client for accessing repository API endpoints</param>
        /// <param name="memoryCache">Memory cache for storing temporary data</param>
        /// <param name="telemetryClient">Client for tracking telemetry and events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public BannersController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            IMemoryCache memoryCache,
            TelemetryClient telemetryClient,
            ILogger<BannersController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var authorizationResult = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.AccessHome);
                if (!authorizationResult.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("Access", "GameServersList", "BannerManagement", null);
                    return Unauthorized();
                }

                TrackSuccessTelemetry("GameServersListAccessed", "GameServersList", new Dictionary<string, string>
                {
                    { "Controller", "Banners" },
                    { "Resource", "GameServersList" },
                    { "Context", "BannerManagement" }
                });

                return View();
            }, "Display game servers list view for banner management");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var authorizationResult = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.AccessHome);
                if (!authorizationResult.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("Access", "GameServersBanners", "BannerData", null);
                    return Unauthorized();
                }

                ApiResult<CollectionModel<GameServerDto>>? gameServersApiResponse;

                if (memoryCache.TryGetValue(GameServersListCacheKey, out gameServersApiResponse) && gameServersApiResponse is not null)
                {
                    Logger.LogDebug("Retrieved game servers data from cache for user {UserId}", User.XtremeIdiotsId());
                }
                else
                {
                    Logger.LogDebug("Fetching game servers data from API for user {UserId}", User.XtremeIdiotsId());

                    gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
                        null, null, GameServerFilter.BannerServerListEnabled, 0, 50,
                        GameServerOrder.BannerServerListPosition, cancellationToken);

                    if (gameServersApiResponse is not null)
                    {
                        memoryCache.Set(GameServersListCacheKey, gameServersApiResponse, DateTime.UtcNow.AddMinutes(5));
                        Logger.LogDebug("Cached game servers data for user {UserId}", User.XtremeIdiotsId());
                    }
                }

                if (gameServersApiResponse?.IsSuccess != true || gameServersApiResponse.Result?.Data?.Items is null)
                {
                    Logger.LogWarning("Failed to retrieve game servers data or data is null for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var htmlBanners = gameServersApiResponse.Result.Data.Items
                    .Where(gs => !string.IsNullOrEmpty(gs.HtmlBanner))
                    .Select(gs => gs.HtmlBanner)
                    .ToList();

                TrackSuccessTelemetry("GameServersBannersRetrieved", "GetGameServers", new Dictionary<string, string>
                {
                    { "Controller", "Banners" },
                    { "Resource", "GameServersBanners" },
                    { "Context", "BannerData" },
                    { "BannerCount", htmlBanners.Count.ToString() }
                });

                return Json(htmlBanners);
            }, "Retrieve game servers banners data");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(queryPort) || string.IsNullOrWhiteSpace(imageName))
                {
                    Logger.LogWarning("User {UserId} provided invalid parameters for GameTracker banner request: IP={IpAddress}, Port={QueryPort}, Image={ImageName}",
                        User.XtremeIdiotsId(), ipAddress, queryPort, imageName);

                    return BadRequest("Invalid parameters provided");
                }

                var authorizationResult = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.AccessHome);
                if (!authorizationResult.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("Access", "GameTrackerBanner", $"IpAddress:{ipAddress},QueryPort:{queryPort},ImageName:{imageName}", null);
                    return Unauthorized();
                }

                var cacheKey = $"{ipAddress}_{queryPort}_{imageName}";
                ApiResult<GameTrackerBannerDto>? repositoryApiResponse;

                if (memoryCache.TryGetValue(cacheKey, out repositoryApiResponse) && repositoryApiResponse is not null)
                {
                    Logger.LogDebug("Retrieved GameTracker banner data from cache for {IpAddress}:{QueryPort}/{ImageName}",
                        ipAddress, queryPort, imageName);
                }
                else
                {
                    Logger.LogDebug("Fetching GameTracker banner data from API for {IpAddress}:{QueryPort}/{ImageName}",
                        ipAddress, queryPort, imageName);

                    repositoryApiResponse = await repositoryApiClient.GameTrackerBanner.V1.GetGameTrackerBanner(
                        ipAddress, queryPort, imageName, cancellationToken);

                    if (repositoryApiResponse is not null)
                    {
                        memoryCache.Set(cacheKey, repositoryApiResponse, DateTime.UtcNow.AddMinutes(30));
                        Logger.LogDebug("Cached GameTracker banner data for {IpAddress}:{QueryPort}/{ImageName}",
                            ipAddress, queryPort, imageName);
                    }
                }

                if (repositoryApiResponse?.IsSuccess != true || repositoryApiResponse.Result?.Data?.BannerUrl is null)
                {
                    var fallbackUrl = $"https://cache.gametracker.com/server_info/{ipAddress}:{queryPort}/{imageName}";

                    Logger.LogWarning("GameTracker banner API failed or returned null data for {IpAddress}:{QueryPort}/{ImageName}, redirecting to fallback: {FallbackUrl}",
                        ipAddress, queryPort, imageName, fallbackUrl);

                    return Redirect(fallbackUrl);
                }

                var bannerUrl = repositoryApiResponse.Result.Data.BannerUrl;

                TrackSuccessTelemetry("GameTrackerBannerRetrieved", "GetGameTrackerBanner", new Dictionary<string, string>
                {
                    { "Controller", "Banners" },
                    { "Resource", "GameTrackerBanner" },
                    { "IpAddress", ipAddress },
                    { "QueryPort", queryPort },
                    { "ImageName", imageName },
                    { "BannerUrl", bannerUrl }
                });

                return Redirect(bannerUrl);
            }, $"Retrieve GameTracker banner for {ipAddress}:{queryPort}/{imageName}");
        }
    }
}