using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Extensions;
using XI.Portal.Players.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.PlayersManagement)]
    public class AdminActionController : Controller
    {
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AdminActionController> _logger;
        private readonly IPlayersRepository _playersRepository;
        private readonly IPortalForumsClient _portalForumsClient;


        public AdminActionController(
            ILogger<AdminActionController> logger,
            IAuthorizationService authorizationService,
            IPlayersRepository playersRepository,
            IAdminActionsRepository adminActionsRepository,
            IPortalForumsClient portalForumsClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _playersRepository = playersRepository ?? throw new ArgumentNullException(nameof(playersRepository));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
            _portalForumsClient = portalForumsClient ?? throw new ArgumentNullException(nameof(portalForumsClient));
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid id, AdminActionType adminActionType)
        {
            var player = await _playersRepository.GetPlayer(id);
            if (player == null) return NotFound();

            var adminAction = new AdminActionDto().OfType(adminActionType).WithPlayerDto(player);
            var canCreateAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

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

            var player = await _playersRepository.GetPlayer(model.PlayerId);
            if (player == null) return NotFound();

            var adminAction = new AdminActionDto().WithPlayerDto(player);
            var canCreateAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            adminAction.AdminId = User.XtremeIdiotsId();
            adminAction.Text = model.Text;
            adminAction.Expires = model.Expires;
            adminAction.Created = DateTime.UtcNow;
            adminAction.ForumTopicId = await _portalForumsClient.CreateTopicForAdminAction(adminAction);

            await _adminActionsRepository.Create(adminAction);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a new {AdminActionType} against {PlayerId}", User.Username(), model.AdminActionType, model.PlayerId);
            TempData["Success"] = $"The {model.AdminActionType} has been successfully created";

            return RedirectToAction("Details", "Players", new {id = model.PlayerId});
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canEditAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

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
            if (adminAction == null) return NotFound();

            var canEditAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            adminAction.Text = model.Text;
            adminAction.Expires = model.Expires;

            var canChangeAdminActionAdmin = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded)
                adminAction.AdminId = model.AdminId;

            await _adminActionsRepository.UpdateAdminAction(adminAction);

            if (adminAction.ForumTopicId != 0)
                await _portalForumsClient.UpdateTopicForAdminAction(adminAction);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has updated {AdminActionId} against {PlayerId}", User.Username(), model.AdminActionId, model.PlayerId);
            TempData["Success"] = $"The {model.AdminActionType} has been successfully updated";

            return RedirectToAction("Details", "Players", new {id = model.PlayerId});
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid id)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canLiftAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

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
            if (adminAction == null) return NotFound();

            var canLiftAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            adminAction.Expires = DateTime.UtcNow;

            await _adminActionsRepository.UpdateAdminAction(adminAction);

            if (adminAction.ForumTopicId != 0)
                await _portalForumsClient.UpdateTopicForAdminAction(adminAction);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has lifted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            TempData["Success"] = "The Admin Action has been successfully updated";

            return RedirectToAction("Details", "Players", new {id = playerId});
        }

        [HttpGet]
        public async Task<IActionResult> Claim(Guid id)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canClaimAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

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
        [ActionName("Claim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClaimConfirmed(Guid id, Guid playerId)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canClaimAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            adminAction.AdminId = User.XtremeIdiotsId();

            await _adminActionsRepository.UpdateAdminAction(adminAction);

            if (adminAction.ForumTopicId != 0)
                await _portalForumsClient.UpdateTopicForAdminAction(adminAction);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has claimed {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            TempData["Success"] = "The Admin Action has been successfully claimed";

            return RedirectToAction("Details", "Players", new {id = playerId});
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscussionTopic(Guid id)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canCreateAdminActionDiscussionTopic = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.CreateAdminActionTopic);

            if (!canCreateAdminActionDiscussionTopic.Succeeded)
                return Unauthorized();

            var topicId = await _portalForumsClient.CreateTopicForAdminAction(adminAction);

            adminAction.ForumTopicId = topicId;

            await _adminActionsRepository.UpdateAdminAction(adminAction);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a discussion topic for {AdminActionId} against {PlayerId}", User.Username(), id, adminAction.PlayerId);
            TempData["Success"] = "The discussion topic has been successfully created";

            return RedirectToAction("Details", "Players", new {id = adminAction.PlayerId});
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canDeleteAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            return View(adminAction);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId)
        {
            var adminAction = await _adminActionsRepository.GetAdminAction(id);
            if (adminAction == null) return NotFound();

            var canDeleteAdminAction = await _authorizationService.AuthorizeAsync(User, adminAction, XtremeIdiotsPolicy.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            await _adminActionsRepository.Delete(id);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has deleted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            TempData["Success"] = "The Admin Action has been successfully deleted";

            return RedirectToAction("Details", "Players", new {id = playerId});
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