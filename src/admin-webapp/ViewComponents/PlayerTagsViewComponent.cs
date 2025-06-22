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
            try
            {
                // Use the Players API endpoint for player tags
                var playerTagsResponse = await repositoryApiClient.Players.GetPlayerTags(playerId);

                if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result == null)
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
                    return View(new List<PlayerTagDto>());
                }

                return View(playerTagsResponse.Result.Entries);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception retrieving player tags for playerId {playerId}: {ex.Message}");

                // Fallback to empty list in case of any error
                return View(new List<PlayerTagDto>());
            }
        }
    }
}
