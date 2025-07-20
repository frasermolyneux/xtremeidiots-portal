using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ViewComponents;

public class PlayerTagsViewComponent : ViewComponent
{
    private readonly IRepositoryApiClient repositoryApiClient;

    public PlayerTagsViewComponent(IRepositoryApiClient repositoryApiClient)
    {
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid playerId)
    {
        try
        {
            var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(playerId);

            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result?.Data?.Items is null)
            {
                if (playerTagsResponse.Result?.Errors is not null && playerTagsResponse.Result.Errors.Any())
                {
                    Console.WriteLine($"Error retrieving player tags for playerId {playerId}: {string.Join(", ", playerTagsResponse.Result.Errors.Select(e => e.Message))}");
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve player tags for playerId {playerId}. Status: {playerTagsResponse.StatusCode}");
                }

                ViewBag.PlayerId = playerId;
                return View(new List<PlayerTagDto>());
            }

            ViewBag.PlayerId = playerId;
            return View(playerTagsResponse.Result.Data.Items);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception retrieving player tags for playerId {playerId}: {ex.Message}");
            ViewBag.PlayerId = playerId;
            return View(new List<PlayerTagDto>());
        }
    }
}