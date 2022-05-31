using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
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
            var gameServerDtos = await RepositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnBannerServerList, 0, 0, GameServerOrder.BannerServerListPosition);

            var filtered = gameServerDtos.Where(s => !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            var htmlBanners = filtered.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }
    }
}