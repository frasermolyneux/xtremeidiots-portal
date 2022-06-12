using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    public class BannersController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        public BannersController(IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        public IActionResult GameServersList()
        {
            return View();
        }

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetGameServers()
        {
            var gameServersApiResponse = await repositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnBannerServerList, 0, 50, GameServerOrder.BannerServerListPosition);

            if (!gameServersApiResponse.IsSuccess || gameServersApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var htmlBanners = gameServersApiResponse.Result.Entries.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }
    }
}