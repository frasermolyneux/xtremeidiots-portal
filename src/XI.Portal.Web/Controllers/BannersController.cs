using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;

namespace XI.Portal.Web.Controllers
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
            var gameServerDtos = await RepositoryApiClient.GameServers.GetGameServers(null, null, "ShowOnBannerServerList", 0, 0, "BannerServerListPosition");

            var filtered = gameServerDtos.Where(s => s.ShowOnBannerServerList && !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            var htmlBanners = filtered.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }
    }
}