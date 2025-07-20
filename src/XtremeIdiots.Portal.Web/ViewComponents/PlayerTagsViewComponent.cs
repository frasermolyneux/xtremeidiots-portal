using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ViewComponents;

/// <summary>
/// View component that displays tags associated with a specific player
/// </summary>
public class PlayerTagsViewComponent(
    IRepositoryApiClient repositoryApiClient,
    ILogger<PlayerTagsViewComponent> logger) : ViewComponent
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    private readonly ILogger<PlayerTagsViewComponent> logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Invokes the view component to display player tags
    /// </summary>
    /// <param name="playerId">The unique identifier of the player whose tags to display</param>
    /// <returns>A view component result containing the player's tags</returns>
    public async Task<IViewComponentResult> InvokeAsync(Guid playerId)
    {
        try
        {
            var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(playerId);

            if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result?.Data?.Items is null)
            {
                if (playerTagsResponse.Result?.Errors is not null && playerTagsResponse.Result.Errors.Length != 0)
                {
                    logger.LogWarning("Error retrieving player tags for playerId {PlayerId}: {Errors}",
                        playerId, string.Join(", ", playerTagsResponse.Result.Errors.Select(e => e.Message)));
                }
                else
                {
                    logger.LogWarning("Failed to retrieve player tags for playerId {PlayerId}. Status: {StatusCode}",
                        playerId, playerTagsResponse.StatusCode);
                }

                ViewBag.PlayerId = playerId;
                return View(new List<PlayerTagDto>());
            }

            ViewBag.PlayerId = playerId;
            return View(playerTagsResponse.Result.Data.Items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception retrieving player tags for playerId {PlayerId}", playerId);
            ViewBag.PlayerId = playerId;
            return View(new List<PlayerTagDto>());
        }
    }
}