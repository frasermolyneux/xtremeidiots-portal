using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.ViewComponents
{
    public class GameServerListViewComponent : ViewComponent
    {
        public GameServerListViewComponent(
            IRepositoryTokenProvider repositoryTokenProvider,
            IRepositoryApiClient repositoryApiClient)
        {
            RepositoryTokenProvider = repositoryTokenProvider;
            RepositoryApiClient = repositoryApiClient;
        }

        public IRepositoryTokenProvider RepositoryTokenProvider { get; }
        public IRepositoryApiClient RepositoryApiClient { get; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var accessToken = await RepositoryTokenProvider.GetRepositoryAccessToken();
            var gameServerDtos = await RepositoryApiClient.GameServers.GetGameServers(accessToken, null, null, "ShowOnBannerServerList", 0, 0, "BannerServerListPosition");

            return View(gameServerDtos);
        }
    }
}