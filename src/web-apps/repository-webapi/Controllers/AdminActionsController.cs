using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class AdminActionsController : Controller
    {
        public AdminActionsController(PortalDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PortalDbContext Context { get; }

        [HttpGet]
        [Route("api/admin-actions/{adminActionId}")]
        public async Task<IActionResult> GetAdminAction(Guid adminActionId)
        {
            var adminAction = await Context.AdminActions
                .Include(aa => aa.PlayerPlayer)
                .Include(aa => aa.UserProfile)
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            if (adminAction == null)
                return NotFound();

            return new OkObjectResult(adminAction.ToDto());
        }

        [HttpGet]
        [Route("api/admin-actions")]
        public async Task<IActionResult> GetAdminActions(GameType? gameType, Guid? playerId, string? adminId, AdminActionFilter? filter, int skipEntries, int takeEntries, AdminActionOrder? order)
        {
            if (order == null)
                order = AdminActionOrder.CreatedDesc;

            var query = Context.AdminActions.Include(aa => aa.PlayerPlayer).Include(aa => aa.UserProfile).AsQueryable();

            if (gameType != null)
                query = query.Where(aa => aa.PlayerPlayer.GameType == ((GameType)gameType).ToGameTypeInt()).AsQueryable();

            if (playerId != null)
                query = query.Where(aa => aa.PlayerPlayerId == playerId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(adminId))
                query = query.Where(aa => aa.UserProfile.XtremeIdiotsForumId == adminId).AsQueryable();

            switch (filter)
            {
                case AdminActionFilter.ActiveBans:
                    query = query.Where(aa => aa.Type == AdminActionType.Ban.ToAdminActionTypeInt() && aa.Expires == null || aa.Type == AdminActionType.TempBan.ToAdminActionTypeInt() && aa.Expires > DateTime.UtcNow).AsQueryable();
                    break;
                case AdminActionFilter.UnclaimedBans:
                    query = query.Where(aa => aa.Type == AdminActionType.Ban.ToAdminActionTypeInt() && aa.Expires == null && aa.UserProfile == null).AsQueryable();
                    break;
            }

            switch (order)
            {
                case AdminActionOrder.CreatedAsc:
                    query = query.OrderBy(aa => aa.Created).AsQueryable();
                    break;
                case AdminActionOrder.CreatedDesc:
                    query = query.OrderByDescending(aa => aa.Created).AsQueryable();
                    break;
            }

            query = query.Skip(skipEntries).AsQueryable();

            if (takeEntries != 0) query = query.Take(takeEntries).AsQueryable();

            var results = await query.ToListAsync();

            var result = results.Select(adminAction => adminAction.ToDto());

            return new OkObjectResult(result);
        }

        [HttpDelete]
        [Route("api/admin-actions/{adminActionId}")]
        public async Task<IActionResult> DeleteAdminAction(Guid adminActionId)
        {
            var adminAction = await Context.AdminActions
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            if (adminAction == null)
                return NotFound();

            Context.Remove(adminAction);
            await Context.SaveChangesAsync();

            return new OkResult();
        }
    }
}
