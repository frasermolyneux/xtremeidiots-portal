using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;

namespace XtremeIdiots.Portal.AdminWebApp.ViewComponents
{
    public class AdminActionsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<AdminActionDto> adminActions, bool linkToPlayer)
        {
            ViewData["LinkToPlayer"] = linkToPlayer;
            return View(adminActions);
        }
    }
}