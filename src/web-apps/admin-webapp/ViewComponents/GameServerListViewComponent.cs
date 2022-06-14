using Microsoft.AspNetCore.Mvc;

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
            var gameServersApiResponse = await RepositoryApiClient.GameServers.GetGameServers(null, null, GameServerFilter.BannerServerListEnabled, 0, 50, GameServerOrder.BannerServerListPosition);

            var filtered = gameServersApiResponse.Result.Entries.Where(s => !string.IsNullOrWhiteSpace(s.HtmlBanner)).ToList();

            return View(filtered);
        }
    }
}