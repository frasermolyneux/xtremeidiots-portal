using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    public class ExternalController : Controller
    {
        public IRepositoryApiClient RepositoryApiClient { get; }

        public ExternalController(IRepositoryApiClient repositoryApiClient)
        {
            RepositoryApiClient = repositoryApiClient;
        }

        public async Task<IActionResult> LatestAdminActions()
        {
            var adminActionDtos = await RepositoryApiClient.AdminActions.GetAdminActions(null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc);

            return View(adminActionDtos);
        }

        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetLatestAdminActions()
        {
            var adminActionDtos = await RepositoryApiClient.AdminActions.GetAdminActions(null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc);

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