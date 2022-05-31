using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.ViewComponents
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
            var gameServerDtos = await RepositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.ShowOnBannerServerList, 0, 0, GameServerOrder.BannerServerListPosition);

            var filtered = gameServerDtos.Where(s => !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            return View(filtered);
        }
    }
}