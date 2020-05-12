using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using XI.CommonTypes;
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

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetLatestAdminActions()
        {
            var filterModel = new AdminActionsFilterModel
            {
                Order = AdminActionsFilterModel.OrderBy.CreatedDesc,
                TakeEntries = 15
            };

            var adminActionDtos = await _adminActionsRepository.GetAdminActions(filterModel);

            var results = new List<dynamic>();
            foreach (var adminActionDto in adminActionDtos)
            {
                string actionText;
                if (adminActionDto.Expires <= DateTime.UtcNow && (adminActionDto.Type == AdminActionType.Ban || adminActionDto.Type == AdminActionType.TempBan))
                    actionText = $"lifted a {adminActionDto.Type} on";
                else
                    actionText = $"added a {adminActionDto.Type} to";

                var adminName = !string.IsNullOrWhiteSpace(adminActionDto.AdminName) ? adminActionDto.AdminName : "Unknown";
                results.Add(new
                {
                    GameIconUrl = $"https://portal.xtremeidiots.com/images/game-icons/{adminActionDto.GameType.ToString()}.png",
                    AdminName = adminName,
                    adminActionDto.AdminId,
                    ActionType = adminActionDto.Type.ToString(),
                    ActionText = actionText,
                    PlayerName = adminActionDto.Username,
                    PlayerLink = $"https://portal.xtremeidiots.com/Players/Details/{adminActionDto.PlayerId}"
                });
            }

            return Json(results);
        }
    }
}