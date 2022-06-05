using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    public class BannersController : Controller
    {
        public BannersController(IRepositoryApiClient repositoryApiClient)
        {
            RepositoryApiClient = repositoryApiClient;
        }

        public IRepositoryApiClient RepositoryApiClient { get; }

        public IActionResult GameServersList()
        {
            return View();
        }

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetGameServers()
        {
            var gameServersApiResponse = await RepositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnBannerServerList, 0, 50, GameServerOrder.BannerServerListPosition);

            var filtered = gameServersApiResponse.Result.Entries.Where(s => !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            var htmlBanners = filtered.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }
    }
}