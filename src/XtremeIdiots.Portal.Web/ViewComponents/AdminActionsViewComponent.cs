using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;

namespace XtremeIdiots.Portal.Web.ViewComponents
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