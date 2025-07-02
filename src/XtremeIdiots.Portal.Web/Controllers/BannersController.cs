using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using MxIO.ApiClient.Abstractions;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameServers;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.GameTracker;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    public class BannersController : Controller
    {
        private const string gameServersListCacheKey = "game-servers-api-response";

        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IMemoryCache memoryCache;

        public BannersController(IRepositoryApiClient repositoryApiClient, IMemoryCache memoryCache)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.memoryCache = memoryCache;
        }

        public IActionResult GameServersList()
        {
            return View();
        }

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetGameServers()
        {
            if (memoryCache.TryGetValue(gameServersListCacheKey, out ApiResponseDto<GameServersCollectionDto>? gameServersApiResponse))
            {
                if (gameServersApiResponse == null)
                    gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(null, null, GameServerFilter.BannerServerListEnabled, 0, 50, GameServerOrder.BannerServerListPosition);
            }
            else
            {
                gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(null, null, GameServerFilter.BannerServerListEnabled, 0, 50, GameServerOrder.BannerServerListPosition);
                memoryCache.Set(gameServersListCacheKey, gameServersApiResponse, DateTime.UtcNow.AddMinutes(5));
            }

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var htmlBanners = gameServersApiResponse.Result.Entries.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }

        [Route("gametracker/{ipAddress}:{queryPort}/{imageName}")]
        public async Task<IActionResult> GetGameTrackerBanner(string ipAddress, string queryPort, string imageName)
        {
            var cacheKey = $"{ipAddress}_{queryPort}_{imageName}";

            if (memoryCache.TryGetValue(cacheKey, out ApiResponseDto<GameTrackerBannerDto>? repositoryApiResponse))
            {
                if (repositoryApiResponse == null)
                    repositoryApiResponse = await repositoryApiClient.GameTrackerBanner.V1.GetGameTrackerBanner(ipAddress, queryPort, imageName);
            }
            else
            {
                repositoryApiResponse = await repositoryApiClient.GameTrackerBanner.V1.GetGameTrackerBanner(ipAddress, queryPort, imageName);
                memoryCache.Set(cacheKey, repositoryApiResponse, DateTime.UtcNow.AddMinutes(30));
            }

            if (!repositoryApiResponse.IsSuccess || repositoryApiResponse.Result == null)
                return Redirect($"https://cache.gametracker.com/server_info/{ipAddress}:{queryPort}/{imageName}");

            return Redirect(repositoryApiResponse.Result.BannerUrl);
        }
    }
}