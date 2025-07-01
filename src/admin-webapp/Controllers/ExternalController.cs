using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient.V1;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    public class ExternalController : Controller
    {
        public IRepositoryApiClient repositoryApiClient { get; }

        public ExternalController(IRepositoryApiClient repositoryApiClient)
        {
            this.repositoryApiClient = repositoryApiClient;
        }

        public async Task<IActionResult> LatestAdminActions()
        {
            var adminActionDtos = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc);

            return View(adminActionDtos);
        }

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetLatestAdminActions()
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result == null)
                return RedirectToAction("Display", "Errors", new { id = 500 });

            var results = new List<dynamic>();
            foreach (var adminActionDto in adminActionsApiResponse.Result.Entries)
            {
                string actionText;
                if (adminActionDto.Expires <= DateTime.UtcNow && (adminActionDto.Type == AdminActionType.Ban || adminActionDto.Type == AdminActionType.TempBan))
                    actionText = $"lifted a {adminActionDto.Type} on";
                else
                    actionText = $"added a {adminActionDto.Type} to";

                var adminName = adminActionDto.UserProfile != null ? adminActionDto.UserProfile.DisplayName : "Unknown";
                var adminId = adminActionDto.UserProfile != null ? adminActionDto.UserProfile.XtremeIdiotsForumId : null;

                results.Add(new
                {
                    GameIconUrl = $"https://portal.xtremeidiots.com/images/game-icons/{adminActionDto.Player?.GameType.ToString()}.png",
                    AdminName = adminName,
                    AdminId = adminId,
                    ActionType = adminActionDto.Type.ToString(),
                    ActionText = actionText,
                    PlayerName = adminActionDto.Player?.Username,
                    PlayerLink = $"https://portal.xtremeidiots.com/Players/Details/{adminActionDto.PlayerId}"
                });
            }

            return Json(results);
        }
    }
}