using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.ViewComponents
{
    public class PlayerTagsViewComponent : ViewComponent
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        public PlayerTagsViewComponent(IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid playerId)
        {
            var playerTagsResponse = await repositoryApiClient.Players.GetPlayerTags(playerId);

            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
                return View(new List<PlayerTagDto>());

            return View(playerTagsResponse.Result.Entries);
        }
    }
}
