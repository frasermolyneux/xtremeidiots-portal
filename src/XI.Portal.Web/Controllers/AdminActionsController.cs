using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Data.Legacy.CommonTypes;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.PlayersManagement)]
    public class AdminActionController : Controller
    {
        private readonly IAdminActionsRepository _adminActionsRepository;

        private readonly string[] _observationWarningKick = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};
        private readonly ILogger<AdminActionController> _logger;
        private readonly IPlayersRepository _playersRepository;
        private readonly string[] _tempBanBan = {XtremeIdiotsClaimTypes.SeniorAdmin, XtremeIdiotsClaimTypes.HeadAdmin, XtremeIdiotsClaimTypes.GameAdmin, XtremeIdiotsClaimTypes.Moderator};

        public AdminActionController(
            ILogger<AdminActionController> logger,
            IPlayersRepository playersRepository,
            IAdminActionsRepository adminActionsRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid? id, AdminActionType adminActionType)
        {
            if (id == null) return NotFound();

            var player = await _playersRepository.GetPlayer((Guid) id, User, _observationWarningKick);

            if (AuthCheck(adminActionType, player.GameType, out var unauthorized)) return unauthorized;

            var model = new CreateAdminActionViewModel
            {
                AdminActionType = adminActionType,
                PlayerId = player.PlayerId,
                PlayerName = player.Username,
                Text = string.Empty,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdminActionViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var player = await _playersRepository.GetPlayer(model.PlayerId, User, _observationWarningKick);

            if (AuthCheck(model.AdminActionType, player.GameType, out var unauthorized)) return unauthorized;

            var currentUserId = User.XtremeIdiotsId();

            var adminAction = new AdminActionDto
            {
                PlayerId = model.PlayerId,
                AdminId = currentUserId,
                Type = model.AdminActionType,
                Text = model.Text,
                Created = DateTime.UtcNow,
                Expires = model.Expires
            };

            await _adminActionsRepository.Create(adminAction);
            // Post Topic
            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a new {AdminActionType} against {PlayerId}", User.Username(), model.AdminActionType, model.PlayerId);
            TempData["Success"] = $"The {model.AdminActionType} has been successfully created";

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var adminAction = await _adminActionsRepository.GetAdminAction((Guid)id);

            if (AuthCheck(adminAction.Type, adminAction.GameType, out var unauthorized)) return unauthorized;

            var canEditAdminAction = User.Claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                                              claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == adminAction.GameType.ToString() ||
                                                              claim.Type == XtremeIdiotsClaimTypes.XtremeIdiotsId && claim.Value == adminAction.AdminId);

            if (!canEditAdminAction) return Unauthorized();

            var model = new EditAdminActionViewModel
            {
                AdminActionId = adminAction.AdminActionId,
                AdminActionType = adminAction.Type,
                PlayerId = adminAction.PlayerId,
                PlayerName = adminAction.Username,
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                AdminId = adminAction.AdminId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAdminActionViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var adminAction = await _adminActionsRepository.GetAdminAction(model.AdminActionId);

            if (AuthCheck(adminAction.Type, adminAction.GameType, out var unauthorized)) return unauthorized;

            var canEditAdminAction = User.Claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                                              claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == adminAction.GameType.ToString() ||
                                                              claim.Type == XtremeIdiotsClaimTypes.XtremeIdiotsId && claim.Value == adminAction.AdminId);

            if (!canEditAdminAction) return Unauthorized();

            var adminActionDto = new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                AdminId = adminAction.AdminId,
                Text = model.Text,
                Expires = model.Expires
            };

            if (User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
            {
                adminActionDto.AdminId = model.AdminId;
            }

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has updated {AdminActionId} against {PlayerId}", User.Username(), model.AdminActionId, model.PlayerId);
            TempData["Success"] = $"The {model.AdminActionType} has been successfully updated";

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid? id)
        {
            if (id == null) return NotFound();

            var adminAction = await _adminActionsRepository.GetAdminAction((Guid)id);

            if (AuthCheck(adminAction.Type, adminAction.GameType, out var unauthorized)) return unauthorized;

            var canEditAdminAction = User.Claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                                              claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == adminAction.GameType.ToString() ||
                                                              claim.Type == XtremeIdiotsClaimTypes.XtremeIdiotsId && claim.Value == adminAction.AdminId);

            if (!canEditAdminAction) return Unauthorized();

            var model = new EditAdminActionViewModel
            {
                AdminActionId = adminAction.AdminActionId,
                AdminActionType = adminAction.Type,
                PlayerId = adminAction.PlayerId,
                PlayerName = adminAction.Username,
                Text = adminAction.Text,
                Expires = adminAction.Expires,
                AdminId = adminAction.AdminId
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("Lift")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiftConfirmed(Guid id, Guid playerId)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);

            if (AuthCheck(adminAction.Type, adminAction.GameType, out var unauthorized)) return unauthorized;

            var canEditAdminAction = User.Claims.Any(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin ||
                                                              claim.Type == XtremeIdiotsClaimTypes.HeadAdmin && claim.Value == adminAction.GameType.ToString() ||
                                                              claim.Type == XtremeIdiotsClaimTypes.XtremeIdiotsId && claim.Value == adminAction.AdminId);

            if (!canEditAdminAction) return Unauthorized();

            var adminActionDto = new AdminActionDto
            {
                AdminActionId = adminAction.AdminActionId,
                AdminId = adminAction.AdminId,
                Text = adminAction.Text,
                Expires = DateTime.UtcNow
            };

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has lifted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            TempData["Success"] = "The Admin Action has been successfully updated";

            return RedirectToAction("Details", "Players", new { id = playerId });
        }


        [HttpGet]
        [Authorize(XtremeIdiotsPolicy.RootPolicy)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _adminActionsRepository.GetAdminAction((Guid) id);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(XtremeIdiotsPolicy.RootPolicy)]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId)
        {
            await _adminActionsRepository.Delete(id);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has deleted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            TempData["Success"] = "The Admin Action has been successfully deleted";

            return RedirectToAction("Details", "Players", new {id = playerId});
        }

        private bool AuthCheck(AdminActionType adminActionType, GameType gameType, out IActionResult unauthorized)
        {
            switch (adminActionType)
            {
                case AdminActionType.Observation:
                    if (!User.HasGameClaim(gameType, _observationWarningKick))
                    {
                        unauthorized = Unauthorized();
                        return true;
                    }

                    break;
                case AdminActionType.Warning:
                    if (!User.HasGameClaim(gameType, _observationWarningKick))
                    {
                        unauthorized = Unauthorized();
                        return true;
                    }

                    break;
                case AdminActionType.Kick:
                    if (!User.HasGameClaim(gameType, _observationWarningKick))
                    {
                        unauthorized = Unauthorized();
                        return true;
                    }

                    break;
                case AdminActionType.TempBan:
                    if (!User.HasGameClaim(gameType, _tempBanBan))
                    {
                        unauthorized = Unauthorized();
                        return true;
                    }

                    break;
                case AdminActionType.Ban:
                    if (!User.HasGameClaim(gameType, _tempBanBan))
                    {
                        unauthorized = Unauthorized();
                        return true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(adminActionType), adminActionType, null);
            }

            unauthorized = null;
            return false;
        }

        public class CreateAdminActionViewModel
        {
            public AdminActionType AdminActionType { get; set; }
            public Guid PlayerId { get; set; }
            public string PlayerName { get; set; }
            public string Text { get; set; }
            public DateTime? Expires { get; set; }
        }

        public class EditAdminActionViewModel
        {
            public Guid AdminActionId { get; set; }
            public AdminActionType AdminActionType { get; set; }
            public Guid PlayerId { get; set; }
            public string PlayerName { get; set; }
            public string Text { get; set; }
            public DateTime? Expires { get; set; }
            public string AdminId { get; set; }
        }
    }
}