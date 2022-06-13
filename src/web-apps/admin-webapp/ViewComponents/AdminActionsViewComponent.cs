using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.Players;

namespace XtremeIdiots.Portal.AdminWebApp.ViewComponents
{
    public class AdminActionsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<AdminActionDto> adminActions, bool linkToPlayer, PlayerDto? playerDto)
        {
            if (playerDto != null)
                ViewData["Player"] = playerDto;

            ViewData["LinkToPlayer"] = linkToPlayer;
            return View(adminActions);
        }
    }
}