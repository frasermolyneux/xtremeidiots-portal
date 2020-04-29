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
using XI.Portal.Web.Extensions;

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
            var playerDto = await _playersRepository.GetPlayer(id);
            if (playerDto == null) return NotFound();

            var adminActionDto = new AdminActionDto().OfType(adminActionType).WithPlayerDto(playerDto);
            var canCreateAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            if (adminActionType == AdminActionType.TempBan)
                adminActionDto.Expires = DateTime.UtcNow.AddDays(7);

            return View(adminActionDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminActionDto model)
        {
            var playerDto = await _playersRepository.GetPlayer(model.PlayerId);
            if (playerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model = model.WithPlayerDto(playerDto);
                return View(model);
            }

            var adminActionDto = new AdminActionDto().OfType(model.Type).WithPlayerDto(playerDto);
            var canCreateAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.AdminId = User.XtremeIdiotsId();
            adminActionDto.Text = model.Text;

            if (model.Type == AdminActionType.TempBan)
                adminActionDto.Expires = model.Expires;

            adminActionDto.ForumTopicId = await _portalForumsClient.CreateTopicForAdminAction(adminActionDto);

            await _adminActionsRepository.CreateAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a new {AdminActionType} against {PlayerId}", User.Username(), model.Type, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully against {playerDto.Username} with a <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">topic</a>");

            return RedirectToAction("Details", "Players", new {id = model.PlayerId});
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canEditAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminActionDto model)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(model.AdminActionId);
            if (adminActionDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Username = adminActionDto.Username;
                return View(model);
            }

            var canEditAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.Text = model.Text;

            if (model.Type == AdminActionType.TempBan)
                adminActionDto.Expires = model.Expires;

            var canChangeAdminActionAdmin = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded)
                adminActionDto.AdminId = model.AdminId;

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await _portalForumsClient.UpdateTopicForAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has updated {AdminActionId} against {PlayerId}", User.Username(), model.AdminActionId, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully updated for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new {id = model.PlayerId});
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canLiftAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Lift")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiftConfirmed(Guid id, Guid playerId)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canLiftAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.Expires = DateTime.UtcNow;

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await _portalForumsClient.UpdateTopicForAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has lifted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully lifted for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new {id = playerId});
        }

        [HttpGet]
        public async Task<IActionResult> Claim(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canClaimAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Claim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClaimConfirmed(Guid id, Guid playerId)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canClaimAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.AdminId = User.XtremeIdiotsId();

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await _portalForumsClient.UpdateTopicForAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has claimed {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully claimed for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new {id = playerId});
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscussionTopic(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canCreateAdminActionDiscussionTopic = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.CreateAdminActionTopic);

            if (!canCreateAdminActionDiscussionTopic.Succeeded)
                return Unauthorized();

            adminActionDto.ForumTopicId = await _portalForumsClient.CreateTopicForAdminAction(adminActionDto);

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a discussion topic for {AdminActionId} against {PlayerId}", User.Username(), id, adminActionDto.PlayerId);
            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction("Details", "Players", new {id = adminActionDto.PlayerId});
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canDeleteAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canDeleteAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, XtremeIdiotsPolicy.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            await _adminActionsRepository.DeleteAdminAction(id);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has deleted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully deleted from {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new {id = playerId});
        }
    }
}