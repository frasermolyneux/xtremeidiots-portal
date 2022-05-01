using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Legacy;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class AdminActionsController : Controller
    {
        public AdminActionsController(LegacyPortalContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public LegacyPortalContext Context { get; }

        [HttpGet]
        [Route("api/admin-actions/{adminActionId}")]
        public async Task<IActionResult> GetAdminAction(Guid adminActionId)
        {
            var adminAction = await Context.AdminActions
                .Include(aa => aa.PlayerPlayer)
                .Include(aa => aa.Admin)
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            var result = new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                PlayerId = adminAction.PlayerPlayer.PlayerId,
                GameType = adminAction.PlayerPlayer.GameType.ToString(),
                Username = adminAction.PlayerPlayer.Username,
                Guid = adminAction.PlayerPlayer.Guid,
                Type = adminAction.Type.ToString(),
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                ForumTopicId = adminAction.ForumTopicId,
                Created = adminAction.Created,
                AdminId = adminAction.Admin?.XtremeIdiotsId,
                AdminName = adminAction.Admin?.UserName
            };

            return new OkObjectResult(result);
        }

        [HttpGet]
        [Route("api/admin-actions")]
        public async Task<IActionResult> GetAdminActions(string? gameType, Guid? playerId, string? adminId, string? filterType, int skipEntries, int takeEntries, string? order)
        {
            if (string.IsNullOrEmpty(order))
                order = "CreatedDesc";

            var query = Context.AdminActions.Include(aa => aa.PlayerPlayer).Include(aa => aa.Admin).AsQueryable();

            if (!string.IsNullOrWhiteSpace(gameType))
                query = query.Where(aa => aa.PlayerPlayer.GameType == Enum.Parse<GameType>(gameType)).AsQueryable();

            if (playerId != null)
                query = query.Where(aa => aa.PlayerPlayerId == playerId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(adminId))
                query = query.Where(aa => aa.Admin.XtremeIdiotsId == adminId).AsQueryable();

            switch (filterType)
            {
                case "ActiveBans":
                    query = query.Where(aa => aa.Type == AdminActionType.Ban && aa.Expires == null || aa.Type == AdminActionType.TempBan && aa.Expires > DateTime.UtcNow).AsQueryable();
                    break;
                case "UnclaimedBans":
                    query = query.Where(aa => aa.Type == AdminActionType.Ban && aa.Expires == null && aa.Admin == null).AsQueryable();
                    break;
            }

            switch (order)
            {
                case "CreatedAsc":
                    query = query.OrderBy(aa => aa.Created).AsQueryable();
                    break;
                case "CreatedDesc":
                    query = query.OrderByDescending(aa => aa.Created).AsQueryable();
                    break;
            }

            query = query.Skip(skipEntries).AsQueryable();

            if (takeEntries != 0) query = query.Take(takeEntries).AsQueryable();

            var results = await query.ToListAsync();

            var result = results.Select(adminAction => new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                PlayerId = adminAction.PlayerPlayer.PlayerId,
                GameType = adminAction.PlayerPlayer.GameType.ToString(),
                Username = adminAction.PlayerPlayer.Username,
                Guid = adminAction.PlayerPlayer.Guid,
                Type = adminAction.Type.ToString(),
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                ForumTopicId = adminAction.ForumTopicId,
                Created = adminAction.Created,
                AdminId = adminAction.Admin?.XtremeIdiotsId,
                AdminName = adminAction.Admin?.UserName
            });

            return new OkObjectResult(result);
        }

        [HttpDelete]
        [Route("api/admin-actions/{adminActionId}")]
        public async Task<IActionResult> DeleteAdminAction(Guid adminActionId)
        {
            var adminAction = await Context.AdminActions
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            if (adminAction == null)
                throw new NullReferenceException(nameof(adminAction));

            Context.Remove(adminAction);
            await Context.SaveChangesAsync();

            return new OkResult();
        }
    }
}
