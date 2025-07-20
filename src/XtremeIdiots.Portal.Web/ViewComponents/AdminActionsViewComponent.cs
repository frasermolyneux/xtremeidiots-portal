using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewComponents;

/// <summary>
/// View component for displaying a list of admin actions with optional player linking
/// </summary>
public class AdminActionsViewComponent : ViewComponent
{
    /// <summary>
    /// Renders the admin actions view component
    /// </summary>
    /// <param name="adminActions">List of admin actions to display</param>
    /// <param name="linkToPlayer">Whether to show links to player profiles</param>
    /// <param name="playerDto">Optional player information for context</param>
    /// <returns>View component result with admin actions data</returns>
    public IViewComponentResult Invoke(List<AdminActionDto> adminActions, bool linkToPlayer, PlayerDto? playerDto)
    {
        if (playerDto is not null)
            ViewData["Player"] = playerDto;

        ViewData["LinkToPlayer"] = linkToPlayer;
        return View(adminActions);
    }
}