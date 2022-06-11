using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.Models;
using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
    public class AdminActionController : Controller
    {
        private readonly ILogger<AdminActionController> logger;
        private readonly IAuthorizationService authorizationService;
        private readonly IAdminActionTopics adminActionTopics;
        private readonly IRepositoryApiClient repositoryApiClient;

        public AdminActionController(
            ILogger<AdminActionController> logger,
            IAuthorizationService authorizationService,
            IAdminActionTopics adminActionTopics,
            IRepositoryApiClient repositoryApiClient)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.adminActionTopics = adminActionTopics ?? throw new ArgumentNullException(nameof(adminActionTopics));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid id, AdminActionType adminActionType)
        {
            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(id);

            if (playerDtoApiResponse.IsNotFound)
                return NotFound();

            var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType>(playerDtoApiResponse.Result.GameType, adminActionType), AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            var viewModel = new AdminActionViewModel
            {
                Type = adminActionType,
                PlayerId = playerDtoApiResponse.Result.Id,
                PlayerDto = playerDtoApiResponse.Result,
                Expires = adminActionType == AdminActionType.TempBan ? DateTime.UtcNow.AddDays(7) : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminActionViewModel model)
        {
            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(model.PlayerId);

            if (playerDtoApiResponse.IsNotFound)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.PlayerDto = playerDtoApiResponse.Result;
                return View(model);
            }

            var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType>(playerDtoApiResponse.Result.GameType, model.Type), AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            var createAdminActionDto = new CreateAdminActionDto(playerDtoApiResponse.Result.Id, model.Type, model.Text)
            {
                AdminId = User.XtremeIdiotsId(),
                Expires = model.Expires,
            };

            createAdminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(model.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.Id, playerDtoApiResponse.Result.Username, DateTime.UtcNow, model.Text, createAdminActionDto.AdminId);

            await repositoryApiClient.AdminActions.CreateAdminAction(createAdminActionDto);

            logger.LogInformation("User {User} has created a new {AdminActionType} against {PlayerId}", User.Username(), model.Type, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully against {playerDtoApiResponse.Result.Username} with a <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{createAdminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">topic</a>");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(adminActionApiResponse.Result.PlayerId);

            var viewModel = new AdminActionViewModel
            {
                AdminActionId = adminActionApiResponse.Result.AdminActionId,
                PlayerId = adminActionApiResponse.Result.PlayerId,
                Type = adminActionApiResponse.Result.Type,
                Text = adminActionApiResponse.Result.Text,
                Expires = adminActionApiResponse.Result.Expires,
                AdminId = adminActionApiResponse.Result.AdminId,
                PlayerDto = playerDtoApiResponse.Result
            };

            var canEditAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType, string>(playerDtoApiResponse.Result.GameType, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.AdminId), AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminActionViewModel model)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(model.AdminActionId);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(adminActionApiResponse.Result.PlayerId);

            if (!ModelState.IsValid)
            {
                model.PlayerDto = playerDtoApiResponse.Result;
                return View(model);
            }

            var canEditAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType, string>(playerDtoApiResponse.Result.GameType, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.AdminId), AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                Text = model.Text
            };

            if (model.Type == AdminActionType.TempBan)
                editAdminActionDto.Expires = model.Expires;

            var canChangeAdminActionAdmin = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.GameType, AuthPolicies.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded)
                editAdminActionDto.AdminId = model.AdminId;

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            if (adminActionApiResponse.Result.ForumTopicId != 0)
                await adminActionTopics.UpdateTopicForAdminAction(adminActionApiResponse.Result.ForumTopicId, adminActionApiResponse.Result.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.Id, playerDtoApiResponse.Result.Username, adminActionApiResponse.Result.Created, model.Text, adminActionApiResponse.Result.AdminId);

            logger.LogInformation("User {User} has updated {AdminActionId} against {PlayerId}", User.Username(), model.AdminActionId, model.PlayerId);
            this.AddAlertSuccess($"The {model.Type} has been successfully updated for {adminActionApiResponse.Result.Username}");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(adminActionApiResponse.Result.GameType, adminActionApiResponse.Result.AdminId), AuthPolicies.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionApiResponse.Result);
        }

        [HttpPost]
        [ActionName("Lift")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LiftConfirmed(Guid id, Guid playerId)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(adminActionApiResponse.Result.PlayerId);

            var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(adminActionApiResponse.Result.GameType, adminActionApiResponse.Result.AdminId), AuthPolicies.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            var editAdminAction = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                Expires = DateTime.UtcNow
            };

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminAction);

            if (adminActionApiResponse.Result.ForumTopicId != 0)
                await adminActionTopics.UpdateTopicForAdminAction(adminActionApiResponse.Result.ForumTopicId, adminActionApiResponse.Result.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.Id, playerDtoApiResponse.Result.Username, adminActionApiResponse.Result.Created, adminActionApiResponse.Result.Text, adminActionApiResponse.Result.AdminId);

            logger.LogInformation("User {User} has lifted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionApiResponse.Result.Type} has been successfully lifted for {adminActionApiResponse.Result.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> Claim(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.GameType, AuthPolicies.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionApiResponse.Result);
        }

        [HttpPost]
        [ActionName("Claim")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClaimConfirmed(Guid id, Guid playerId)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse == null)
                return NotFound();

            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(adminActionApiResponse.Result.PlayerId);

            var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.GameType, AuthPolicies.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                AdminId = User.XtremeIdiotsId()
            };

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            if (adminActionApiResponse.Result.ForumTopicId != 0)
                await adminActionTopics.UpdateTopicForAdminAction(adminActionApiResponse.Result.ForumTopicId, adminActionApiResponse.Result.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.Id, playerDtoApiResponse.Result.Username, adminActionApiResponse.Result.Created, adminActionApiResponse.Result.Text, adminActionApiResponse.Result.AdminId);

            logger.LogInformation("User {User} has claimed {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionApiResponse.Result.Type} has been successfully claimed for {adminActionApiResponse.Result.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscussionTopic(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var playerDtoApiResponse = await repositoryApiClient.Players.GetPlayer(adminActionApiResponse.Result.PlayerId);

            var canCreateAdminActionDiscussionTopic = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.GameType, AuthPolicies.CreateAdminActionTopic);

            if (!canCreateAdminActionDiscussionTopic.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(adminActionApiResponse.Result.Type, playerDtoApiResponse.Result.GameType, playerDtoApiResponse.Result.Id, playerDtoApiResponse.Result.Username, DateTime.UtcNow, adminActionApiResponse.Result.Text, adminActionApiResponse.Result.AdminId)
            };

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            logger.LogInformation("User {User} has created a discussion topic for {AdminActionId} against {PlayerId}", User.Username(), id, adminActionApiResponse.Result.PlayerId);
            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionApiResponse.Result.ForumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction("Details", "Players", new { id = adminActionApiResponse.Result.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {

            var adminActionDto = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionDto == null)
                return NotFound();

            var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionDto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound)
                return NotFound();

            var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            await repositoryApiClient.AdminActions.DeleteAdminAction(id);

            logger.LogInformation("User {User} has deleted {AdminActionId} against {PlayerId}", User.Username(), id, playerId);
            this.AddAlertSuccess($"The {adminActionApiResponse.Result.Type} has been successfully deleted from {adminActionApiResponse.Result.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }
    }
}