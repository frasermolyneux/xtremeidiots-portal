using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Players.Interfaces;
using XI.Portal.Players.Models;

namespace XI.Portal.Web.Controllers
{
    public class ExternalController : Controller
    {
        private readonly IAdminActionsRepository _adminActionsRepository;

        public ExternalController(IAdminActionsRepository adminActionsRepository)
        {
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
        }

        public async Task<IActionResult> LatestAdminActions()
        {
            var filterModel = new AdminActionsFilterModel
            {
                Order = AdminActionsFilterModel.OrderBy.CreatedDesc,
                TakeEntries = 15
            };

            var adminActionDtos = await _adminActionsRepository.GetAdminActions(filterModel);

            return View(adminActionDtos);
        }
    }
}