using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Extensions;
using XI.Portal.Players.Interfaces;
using XI.Portal.Web.Extensions;
using XI.Portal.Web.Models;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.NetStandard.Models;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard;
using XtremeIdiots.Portal.RepositoryApiClient.NetStandard.Providers;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
    public class AdminActionController : Controller
    {
        private readonly ILogger<AdminActionController> logger;
        private readonly IAuthorizationService authorizationService;
        private readonly IPlayersForumsClient playersForumsClient;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly IRepositoryTokenProvider repositoryTokenProvider;

        public AdminActionController(
            ILogger<AdminActionController> logger,
            IAuthorizationService authorizationService,
            IPlayersForumsClient playersForumsClient,
            IRepositoryApiClient repositoryApiClient,
            IRepositoryTokenProvider repositoryTokenProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.playersForumsClient = playersForumsClient ?? throw new ArgumentNullException(nameof(playersForumsClient));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.repositoryTokenProvider = repositoryTokenProvider ?? throw new ArgumentNullException(nameof(repositoryTokenProvider));
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid id, AdminActionType adminActionType)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var playerDto = await repositoryApiClient.Players.GetPlayer(accessToken, id);

            if (playerDto == null)
                return NotFound();

            var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, new AdminActionDto { GameType = playerDto.GameType }, AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            var viewModel = new AdminActionViewModel
            {
                Type = adminActionType,
                PlayerId = playerDto.Id,
                PlayerDto = playerDto,
                Expires = adminActionType == AdminActionType.TempBan ? DateTime.UtcNow.AddDays(7) : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminActionViewModel model)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var playerDto = await repositoryApiClient.Players.GetPlayer(accessToken, model.PlayerId);

            if (playerDto == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.PlayerDto = playerDto;
                return View(model);
            }

            var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, new AdminActionDto { GameType = playerDto.GameType }, AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            var adminActionDto = new AdminActionDto
            {
                Type = model.Type,
                PlayerId = playerDto.Id,
                GameType = playerDto.GameType,
                Username = playerDto.Username,
                Guid = playerDto.Guid,
                AdminId = User.XtremeIdiotsId(),
                Text = model.Text,
                Expires = model.Expires
            };

            adminActionDto.ForumTopicId = await playersForumsClient.CreateTopicForAdminAction(adminActionDto);

            await repositoryApiClient.Players.CreateAdminActionForPlayer(accessToken, adminActionDto);

            logger.LogInformation("User {User} has created a new {AdminActionType} against {PlayerId}", User.Username(), model.Type, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully against {playerDto.Username} with a <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">topic</a>");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);
            var playerDto = await repositoryApiClient.Players.GetPlayer(accessToken, adminActionDto.PlayerId);

            if (adminActionDto == null)
                return NotFound();

            var viewModel = new AdminActionViewModel
            {
                AdminActionId = adminActionDto.AdminActionId,
                PlayerId = adminActionDto.PlayerId,
                Type = adminActionDto.Type,
                Text = adminActionDto.Text,
                Expires = adminActionDto.Expires,
                AdminId = adminActionDto.AdminId,
                PlayerDto = playerDto
            };

            var canEditAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminActionViewModel model)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, model.AdminActionId);
            var playerDto = await repositoryApiClient.Players.GetPlayer(accessToken, adminActionDto.PlayerId);

            if (adminActionDto == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.PlayerDto = playerDto;
                return View(model);
            }

            var canEditAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.Text = model.Text;

            if (model.Type == AdminActionType.TempBan)
                adminActionDto.Expires = model.Expires;

            var canChangeAdminActionAdmin = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded)
                adminActionDto.AdminId = model.AdminId;

            await repositoryApiClient.Players.UpdateAdminActionForPlayer(accessToken, adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await playersForumsClient.UpdateTopicForAdminAction(adminActionDto);

            logger.LogInformation("User {User} has updated {AdminActionId} against {PlayerId}", User.Username(), model.AdminActionId, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully updated for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Lift")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiftConfirmed(Guid id, Guid playerId)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.Expires = DateTime.UtcNow;

            await repositoryApiClient.Players.UpdateAdminActionForPlayer(accessToken, adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await playersForumsClient.UpdateTopicForAdminAction(adminActionDto);

            logger.LogInformation("User {User} has lifted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully lifted for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> Claim(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Claim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClaimConfirmed(Guid id, Guid playerId)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            adminActionDto.AdminId = User.XtremeIdiotsId();

            await repositoryApiClient.Players.UpdateAdminActionForPlayer(accessToken, adminActionDto);

            if (adminActionDto.ForumTopicId != 0)
                await playersForumsClient.UpdateTopicForAdminAction(adminActionDto);

            logger.LogInformation("User {User} has claimed {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully claimed for {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscussionTopic(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canCreateAdminActionDiscussionTopic = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.CreateAdminActionTopic);

            if (!canCreateAdminActionDiscussionTopic.Succeeded)
                return Unauthorized();

            adminActionDto.ForumTopicId = await playersForumsClient.CreateTopicForAdminAction(adminActionDto);

            await repositoryApiClient.Players.UpdateAdminActionForPlayer(accessToken, adminActionDto);

            logger.LogInformation("User {User} has created a discussion topic for {AdminActionId} against {PlayerId}", User.Username(), id, adminActionDto.PlayerId);
            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction("Details", "Players", new { id = adminActionDto.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId)
        {
            var accessToken = await repositoryTokenProvider.GetRepositoryAccessToken();
            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(accessToken, id);

            if (adminActionDto == null)
                return NotFound();

            var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, adminActionDto, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            await repositoryApiClient.AdminActions.DeleteAdminAction(accessToken, id);

            logger.LogInformation("User {User} has deleted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionDto.Type} has been successfully deleted from {adminActionDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }
    }
}