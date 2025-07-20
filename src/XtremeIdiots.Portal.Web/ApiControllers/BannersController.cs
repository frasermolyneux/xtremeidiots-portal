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

namespace XtremeIdiots.Portal.Web.ApiControllers;

[Authorize(Policy = AuthPolicies.AccessHome)]
[Route("Banners")]
public class BannersController : BaseApiController
{
    private const string GameServersListCacheKey = nameof(GameServersListCacheKey);
    private readonly IAuthorizationService authorizationService;
    private readonly IRepositoryApiClient repositoryApiClient;
    private readonly IMemoryCache memoryCache;

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

    [HttpGet("GetGameServers")]
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

            if (memoryCache.TryGetValue(GameServersListCacheKey, out gameServersApiResponse) && gameServersApiResponse != null)
            {
                Logger.LogDebug("Retrieved game servers data from cache for user {UserId}", User.XtremeIdiotsId());
            }
            else
            {
                Logger.LogDebug("Fetching game servers data from API for user {UserId}", User.XtremeIdiotsId());

                gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
         null, null, GameServerFilter.BannerServerListEnabled, 0, 50,
         GameServerOrder.BannerServerListPosition, cancellationToken);

                if (gameServersApiResponse != null)
                {
                    memoryCache.Set(GameServersListCacheKey, gameServersApiResponse, DateTime.UtcNow.AddMinutes(5));
                    Logger.LogDebug("Cached game servers data for user {UserId}", User.XtremeIdiotsId());
                }
            }

            if (gameServersApiResponse?.IsSuccess != true || gameServersApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve game servers data or data is null for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500, "Failed to retrieve game servers banner data");
            }

            var htmlBanners = gameServersApiResponse.Result.Data.Items
     .Where(gs => !string.IsNullOrEmpty(gs.HtmlBanner))
     .Select(gs => gs.HtmlBanner)
     .ToList();

            TrackSuccessTelemetry("GameServersBannersRetrieved", nameof(GetGameServers), new Dictionary<string, string>
        {
 { "Controller", nameof(BannersController).Replace("Controller", string.Empty) },
 { "Resource", "GameServersBanners" },
 { "Context", "BannerData" },
 { "BannerCount", htmlBanners.Count.ToString() }
        });

            return Ok(htmlBanners);
        }, "Retrieve game servers banners data");
    }

    [HttpGet("gametracker/{ipAddress}:{queryPort}/{imageName}")]
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

            if (memoryCache.TryGetValue(cacheKey, out repositoryApiResponse) && repositoryApiResponse != null)
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

                if (repositoryApiResponse != null)
                {
                    memoryCache.Set(cacheKey, repositoryApiResponse, DateTime.UtcNow.AddMinutes(5));
                    Logger.LogDebug("Cached GameTracker banner data for {IpAddress}:{QueryPort}/{ImageName}",
             ipAddress, queryPort, imageName);
                }
            }

            if (repositoryApiResponse?.IsSuccess == true && repositoryApiResponse.Result?.Data != null)
            {
                var bannerData = repositoryApiResponse.Result.Data;

                Logger.LogInformation("Successfully retrieved GameTracker banner for {IpAddress}:{QueryPort}/{ImageName}, redirecting to {BannerUrl}",
         ipAddress, queryPort, imageName, bannerData.BannerUrl);

                TrackSuccessTelemetry("GameTrackerBannerRetrieved", nameof(GetGameTrackerBanner), new Dictionary<string, string>
        {
 { "Controller", nameof(BannersController).Replace("Controller", string.Empty) },
 { "Resource", "GameTrackerBanner" },
 { "IpAddress", ipAddress },
 { "QueryPort", queryPort },
 { "ImageName", imageName },
 { "BannerUrl", bannerData.BannerUrl ?? "null" }
        });

                return Redirect(bannerData.BannerUrl ?? $"https://cache.gametracker.com/server_info/{ipAddress}:{queryPort}/{imageName}");
            }

            Logger.LogWarning("Failed to retrieve GameTracker banner data for {IpAddress}:{QueryPort}/{ImageName}, falling back to default GameTracker URL",
     ipAddress, queryPort, imageName);

            TrackSuccessTelemetry("GameTrackerBannerFallback", nameof(GetGameTrackerBanner), new Dictionary<string, string>
        {
 { "Controller", nameof(BannersController).Replace("Controller", string.Empty) },
 { "Resource", "GameTrackerBanner" },
 { "IpAddress", ipAddress },
 { "QueryPort", queryPort },
 { "ImageName", imageName },
 { "Fallback", "true" }
        });

            return Redirect($"https://cache.gametracker.com/server_info/{ipAddress}:{queryPort}/{imageName}");
        }, nameof(GetGameTrackerBanner), $"ipAddress: {ipAddress}, queryPort: {queryPort}, imageName: {imageName}");
    }
}