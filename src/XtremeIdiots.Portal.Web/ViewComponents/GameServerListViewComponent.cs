using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ViewComponents;

/// <summary>
/// View component that displays a list of game servers with banners enabled
/// </summary>
/// <remarks>
/// Initializes a new instance of the GameServerListViewComponent
/// </remarks>
/// <param name="repositoryApiClient">Client for repository API operations</param>
public class GameServerListViewComponent(IRepositoryApiClient repositoryApiClient) : ViewComponent
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Retrieves and displays game servers that have banner functionality enabled
    /// </summary>
    /// <returns>View result with filtered game servers that have HTML banners</returns>
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