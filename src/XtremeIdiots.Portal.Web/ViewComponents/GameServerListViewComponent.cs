using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ViewComponents;

public class GameServerListViewComponent : ViewComponent
{
    private readonly IRepositoryApiClient repositoryApiClient;

    public GameServerListViewComponent(IRepositoryApiClient repositoryApiClient)
    {
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var gameServersApiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(
            null, null, GameServerFilter.BannerServerListEnabled, 0, 50, GameServerOrder.BannerServerListPosition);

        if (gameServersApiResponse.Result?.Data?.Items is null)
        {
            return View(new List<object>());
        }

        var filtered = gameServersApiResponse.Result.Data.Items
            .Where(s => !string.IsNullOrWhiteSpace(s.HtmlBanner))
            .ToList();

        return View(filtered);
    }
}