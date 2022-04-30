using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using XI.CommonTypes;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Players.Dto;
using XI.Portal.Players.Extensions;
using XI.Portal.Players.Interfaces;
using XI.Portal.Web.Extensions;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
    public class AdminActionController : Controller
    {
        private readonly IAdminActionsRepository _adminActionsRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AdminActionController> _logger;
        private readonly IPlayersForumsClient _playersForumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;


        public AdminActionController(
            ILogger<AdminActionController> logger,
            IAuthorizationService authorizationService,
            IAdminActionsRepository adminActionsRepository,
            IPlayersForumsClient playersForumsClient,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _adminActionsRepository = adminActionsRepository ?? throw new ArgumentNullException(nameof(adminActionsRepository));
            _playersForumsClient = playersForumsClient ?? throw new ArgumentNullException(nameof(playersForumsClient));
            this.repositoryApiClient = repositoryApiClient;
            this.repositoryTokenProvider = repositoryTokenProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid id, AdminActionType adminActionType)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var playerDto = await repositoryApiClient.Players.GetPlayer(accessToken, id);

            if (playerDto == null) return NotFound();

            var adminActionDto = new AdminActionDto().OfType(adminActionType);
            adminActionDto.PlayerId = playerDto.Id;
            adminActionDto.GameType = Enum.Parse<GameType>(playerDto.GameType);
            adminActionDto.Username = playerDto.Username;
            adminActionDto.Guid = playerDto.Guid;

            var canCreateAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.CreateAdminAction);

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
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var playerDto = await repositoryApiClient.Players.GetPlayer(accessToken, model.PlayerId);

            if (playerDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.PlayerId = playerDto.Id;
                model.GameType = Enum.Parse<GameType>(playerDto.GameType);
                model.Username = playerDto.Username;
                model.Guid = playerDto.Guid;

                return View(model);
            }

            var adminActionDto = new AdminActionDto().OfType(model.Type);
            adminActionDto.PlayerId = playerDto.Id;
            adminActionDto.GameType = Enum.Parse<GameType>(playerDto.GameType);
            adminActionDto.Username = playerDto.Username;
            adminActionDto.Guid = playerDto.Guid;

            var canCreateAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.AdminId = User.XtremeIdiotsId();
            adminActionDto.Text = model.Text;

            if (model.Type == AdminActionType.TempBan)
                adminActionDto.Expires = model.Expires;

            adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

            await _adminActionsRepository.CreateAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a new {AdminActionType} against {PlayerId}", User.Username(), model.Type, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully against {playerDto.Username} with a <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">topic</a>");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canEditAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.EditAdminAction);

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

            var canEditAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.Text = model.Text;

            if (model.Type == AdminActionType.TempBan)
                adminActionDto.Expires = model.Expires;

            var canChangeAdminActionAdmin = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded)
                adminActionDto.AdminId = model.AdminId;

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await _playersForumsClient.UpdateTopicForAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has updated {AdminActionId} against {PlayerId}", User.Username(), model.AdminActionId, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully updated for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canLiftAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.LiftAdminAction);

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

            var canLiftAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.Expires = DateTime.UtcNow;

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await _playersForumsClient.UpdateTopicForAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has lifted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully lifted for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> Claim(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canClaimAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.ClaimAdminAction);

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

            var canClaimAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.AdminId = User.XtremeIdiotsId();

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await _playersForumsClient.UpdateTopicForAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has claimed {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully claimed for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscussionTopic(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canCreateAdminActionDiscussionTopic = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.CreateAdminActionTopic);

            if (!canCreateAdminActionDiscussionTopic.Succeeded)
                return Unauthorized();

            adminActionDto.ForumTopicId = await _playersForumsClient.CreateTopicForAdminAction(adminActionDto);

            await _adminActionsRepository.UpdateAdminAction(adminActionDto);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has created a discussion topic for {AdminActionId} against {PlayerId}", User.Username(), id, adminActionDto.PlayerId);
            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction("Details", "Players", new { id = adminActionDto.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var adminActionDto = await _adminActionsRepository.GetAdminAction(id);
            if (adminActionDto == null) return NotFound();

            var canDeleteAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.DeleteAdminAction);

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

            var canDeleteAdminAction = await _authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            await _adminActionsRepository.DeleteAdminAction(id);

            _logger.LogInformation(EventIds.AdminAction, "User {User} has deleted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully deleted from {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }
    }
}