using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XI.Portal.Web.ViewComponents
{
    public class GameServerListViewComponent : ViewComponent
    {
        public GameServerListViewComponent(IRepositoryApiClient repositoryApiClient)
        {
            RepositoryApiClient = repositoryApiClient;
        }

        public IRepositoryApiClient RepositoryApiClient { get; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var gameServerDtos = await RepositoryApiClient.GameServers.GetGameServers(null, null, "ShowOnBannerServerList", 0, 0, "BannerServerListPosition");

            var filtered = gameServerDtos.Where(s => s.ShowOnBannerServerList && !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            return View(filtered);
        }
    }
}