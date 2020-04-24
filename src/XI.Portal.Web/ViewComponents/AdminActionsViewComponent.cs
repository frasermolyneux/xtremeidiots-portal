using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Players.Dto;

namespace XI.Portal.Web.ViewComponents
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