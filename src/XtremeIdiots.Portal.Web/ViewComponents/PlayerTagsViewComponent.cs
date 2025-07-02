using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Tags;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.Web.ViewComponents
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
            try
            {
                // Use the Players API endpoint for player tags
                var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(playerId); if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
                {
                    // Log the error if available
                    if (playerTagsResponse.Errors != null && playerTagsResponse.Errors.Any())
                    {
                        // Log the error
                        Console.WriteLine($"Error retrieving player tags for playerId {playerId}: {string.Join(", ", playerTagsResponse.Errors)}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve player tags for playerId {playerId}. Status: {playerTagsResponse.StatusCode}");
                    }
                    ViewBag.PlayerId = playerId;
                    return View(new List<PlayerTagDto>());
                }

                ViewBag.PlayerId = playerId;
                return View(playerTagsResponse.Result.Entries);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception retrieving player tags for playerId {playerId}: {ex.Message}");

                // Fallback to empty list in case of any error
                ViewBag.PlayerId = playerId;
                return View(new List<PlayerTagDto>());
            }
        }
    }
}
