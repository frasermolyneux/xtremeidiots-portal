using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using XtremeIdiots.Portal.DataLib;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryWebApi.Extensions;

namespace XtremeIdiots.Portal.RepositoryWebApi.Controllers
{
    [ApiController]
    [Authorize(Roles = "ServiceAccount")]
    public class AdminActionsController : Controller
    {
        private readonly ILogger<AdminActionsController> logger;
        private readonly PortalDbContext context;

        public AdminActionsController(ILogger<AdminActionsController> logger, PortalDbContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        [Route("api/admin-actions/{adminActionId}")]
        public async Task<IActionResult> GetAdminAction(Guid adminActionId)
        {
            var adminAction = await context.AdminActions
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

            var query = context.AdminActions.Include(aa => aa.PlayerPlayer).Include(aa => aa.UserProfile).AsQueryable();

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
            var adminAction = await context.AdminActions
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            if (adminAction == null)
                return NotFound();

            context.Remove(adminAction);
            await context.SaveChangesAsync();

            return new OkResult();
        }

        [HttpGet]
        [Route("api/players/{playerId}/admin-actions")]
        public async Task<IActionResult> GetAdminActionsForPlayer(Guid playerId)
        {
            var results = await context.AdminActions
                .Include(aa => aa.PlayerPlayer)
                .Include(aa => aa.UserProfile)
                .Where(aa => aa.PlayerPlayerId == playerId)
                .OrderByDescending(aa => aa.Created)
                .ToListAsync();

            var result = results.Select(adminAction => adminAction.ToDto());

            return new OkObjectResult(result);
        }

        [HttpPost]
        [Route("api/players/{playerId}/admin-actions")]
        public async Task<IActionResult> CreateAdminActionForPlayer(Guid playerId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            AdminActionDto adminActionDto;
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                adminActionDto = JsonConvert.DeserializeObject<AdminActionDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (adminActionDto == null) return new BadRequestResult();
            if (adminActionDto.PlayerId != playerId) return new BadRequestResult();

            var player = await context.Players.SingleOrDefaultAsync(p => p.PlayerId == adminActionDto.PlayerId);

            UserProfile admin = null;
            if (!string.IsNullOrWhiteSpace(adminActionDto.AdminId))
            {
                admin = await context.UserProfiles.SingleOrDefaultAsync(u => u.XtremeIdiotsForumId == adminActionDto.AdminId);
            }

            var adminAction = new AdminAction
            {
                PlayerPlayer = player,
                UserProfile = admin,
                Type = adminActionDto.Type.ToAdminActionTypeInt(),
                Text = adminActionDto.Text,
                Created = DateTime.UtcNow,
                Expires = adminActionDto.Expires,
                ForumTopicId = adminActionDto.ForumTopicId
            };

            context.AdminActions.Add(adminAction);
            await context.SaveChangesAsync();

            return new OkObjectResult(adminActionDto);
        }

        [HttpPatch]
        [Route("api/players/{playerId}/admin-actions/{adminActionId}")]
        public async Task<IActionResult> UpdateAdminActionForPlayer(Guid playerId, Guid adminActionId)
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            AdminActionDto adminActionDto;
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                adminActionDto = JsonConvert.DeserializeObject<AdminActionDto>(requestBody);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not deserialize request body");
                return new BadRequestResult();
            }

            if (adminActionDto == null) return new BadRequestResult();
            if (adminActionDto.PlayerId != playerId) return new BadRequestResult();

            var adminAction = await context.AdminActions
                .Include(aa => aa.UserProfile)
                .SingleOrDefaultAsync(aa => aa.AdminActionId == adminActionId);

            if (adminAction == null)
                throw new NullReferenceException(nameof(adminAction));

            adminAction.Text = adminActionDto.Text;
            adminAction.Expires = adminActionDto.Expires;

            if (adminAction.UserProfile.XtremeIdiotsForumId != adminActionDto.AdminId)
            {
                if (string.IsNullOrWhiteSpace(adminActionDto.AdminId))
                    adminAction.UserProfile = null;
                else
                {
                    var admin = await context.UserProfiles.SingleOrDefaultAsync(u => u.XtremeIdiotsForumId == adminActionDto.AdminId);

                    if (admin == null)
                        throw new NullReferenceException(nameof(admin));

                    adminAction.UserProfile = admin;
                }
            }

            if (adminActionDto.ForumTopicId != 0)
                adminAction.ForumTopicId = adminActionDto.ForumTopicId;

            await context.SaveChangesAsync();

            return new OkObjectResult(adminActionDto);
        }
    }
}
