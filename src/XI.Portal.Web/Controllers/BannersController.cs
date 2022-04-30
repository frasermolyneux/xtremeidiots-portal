using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    public class BannersController : Controller
    {
        public BannersController(
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            RepositoryTokenProvider = repositoryTokenProvider;
            RepositoryApiClient = repositoryApiClient;
        }

        public IRepositoryTokenProvider RepositoryTokenProvider { get; }
        public IRepositoryApiClient RepositoryApiClient { get; }

        public IActionResult GameServersList()
        {
            return View();
        }

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetGameServers()
        {
            var accessToken = await RepositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDtos = await RepositoryApiClient.GameServers.GetGameServers(accessToken, null, null, "ShowOnBannerServerList", 0, 0, "BannerServerListPosition");

            var filtered = gameServerDtos.Where(s => s.ShowOnBannerServerList && !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            var htmlBanners = filtered.Select(gs => gs.HtmlBanner).ToList();

            return Json(htmlBanners);
        }
    }
}