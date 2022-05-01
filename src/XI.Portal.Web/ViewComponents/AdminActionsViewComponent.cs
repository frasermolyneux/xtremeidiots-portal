using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;

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