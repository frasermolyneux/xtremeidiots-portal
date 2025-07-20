using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using MX.Api.Abstractions;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameServers;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.GameTracker;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing and serving game server banners and GameTracker integration
/// </summary>
[Authorize(Policy = AuthPolicies.AccessHome)]
public class BannersController(
    IAuthorizationService authorizationService,
    IRepositoryApiClient repositoryApiClient,
    IMemoryCache memoryCache,
    TelemetryClient telemetryClient,
    ILogger<BannersController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private const string GameServersListCacheKey = nameof(GameServersListCacheKey);

    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    private readonly IMemoryCache memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

    /// <summary>
    /// Displays the game servers list view for banner management
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View for displaying game servers list</returns>
    [HttpGet]
    public async Task<IActionResult> GameServersList(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.AccessHome);
            if (!authorizationResult.Succeeded)
            {
                TrackUnauthorizedAccessAttempt("Access", nameof(GameServersList), "BannerManagement", null);
                return Unauthorized();
            }

            TrackSuccessTelemetry("GameServersListAccessed", nameof(GameServersList), new Dictionary<string, string>
            {
                { "Controller", nameof(BannersController) },
                { "Resource", nameof(GameServersList) },
                { "Context", "BannerManagement" }
            });

            return View();
        }, "Display game servers list view for banner management");
    }

    /// <summary>
    /// Retrieves game servers banner data as JSON for external consumption
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>JSON array of HTML banners from enabled game servers</returns>
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

            if (memoryCache.TryGetValue(GameServersListCacheKey, out
            ApiResult<CollectionModel<GameServerDto>>? gameServersApiResponse) && gameServersApiResponse is not null)
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
                return RedirectToAction(nameof(ErrorsController.Display), "Errors", new { id = 500 });
            }

            var htmlBanners = gameServersApiResponse.Result.Data.Items
                .Where(gs => !string.IsNullOrEmpty(gs.HtmlBanner))
                .Select(gs => gs.HtmlBanner)
                .ToList();

            TrackSuccessTelemetry("GameServersBannersRetrieved", nameof(GetGameServers), new Dictionary<string, string>
            {
                { "Controller", nameof(BannersController) },
                { "Resource", "GameServersBanners" },
                { "Context", "BannerData" },
                { "BannerCount", htmlBanners.Count.ToString() }
            });

            return Json(htmlBanners);
        }, "Retrieve game servers banners data");
    }

    /// <summary>
    /// Retrieves GameTracker banner for a specific game server by redirecting to the banner URL
    /// </summary>
    /// <param name="ipAddress">Server IP address</param>
    /// <param name="queryPort">Server query port</param>
    /// <param name="imageName">Banner image name/type</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirect to GameTracker banner URL or fallback URL</returns>
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

            if (memoryCache.TryGetValue(cacheKey, out ApiResult<GameTrackerBannerDto>? repositoryApiResponse) && repositoryApiResponse is not null)
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

            TrackSuccessTelemetry("GameTrackerBannerRetrieved", nameof(GetGameTrackerBanner), new Dictionary<string, string>
            {
                { "Controller", nameof(BannersController) },
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