using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.AdminWebApp.Auth.Constants;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.AdminWebApp.ViewModels;
using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Models.AdminActions;
using XtremeIdiots.Portal.RepositoryApiClient;

namespace XtremeIdiots.Portal.AdminWebApp.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
    public class AdminActionController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IAdminActionTopics adminActionTopics;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;

        public AdminActionController(
            IAuthorizationService authorizationService,
            IAdminActionTopics adminActionTopics,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.adminActionTopics = adminActionTopics ?? throw new ArgumentNullException(nameof(adminActionTopics));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid id, AdminActionType adminActionType)
        {
            var playerApiResponse = await repositoryApiClient.Players.GetPlayer(id);

            if (playerApiResponse.IsNotFound || playerApiResponse.Result == null)
                return NotFound();

            var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType>(playerApiResponse.Result.GameType, adminActionType), AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            var viewModel = new CreateAdminActionViewModel
            {
                Type = adminActionType,
                PlayerId = playerApiResponse.Result.PlayerId,
                PlayerDto = playerApiResponse.Result,
                Expires = adminActionType == AdminActionType.TempBan ? DateTime.UtcNow.AddDays(7) : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAdminActionViewModel model)
        {
            var playerApiResponse = await repositoryApiClient.Players.GetPlayer(model.PlayerId);

            if (playerApiResponse.IsNotFound || playerApiResponse.Result == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.PlayerDto = playerApiResponse.Result;
                return View(model);
            }

            var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType>(playerApiResponse.Result.GameType, model.Type), AuthPolicies.CreateAdminAction);

            if (!canCreateAdminAction.Succeeded)
                return Unauthorized();

            var createAdminActionDto = new CreateAdminActionDto(playerApiResponse.Result.PlayerId, model.Type, model.Text)
            {
                AdminId = User.XtremeIdiotsId(),
                Expires = model.Expires,
            };

            createAdminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(model.Type, playerApiResponse.Result.GameType, playerApiResponse.Result.PlayerId, playerApiResponse.Result.Username, DateTime.UtcNow, model.Text, createAdminActionDto.AdminId);

            await repositoryApiClient.AdminActions.CreateAdminAction(createAdminActionDto);

            var eventTelemetry = new EventTelemetry("CreateAdminAction").Enrich(User).Enrich(playerApiResponse.Result).Enrich(createAdminActionDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The {model.Type} has been successfully against {playerApiResponse.Result.Username} with a <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{createAdminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">topic</a>");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var viewModel = new EditAdminActionViewModel
            {
                AdminActionId = adminActionApiResponse.Result.AdminActionId,
                PlayerId = adminActionApiResponse.Result.PlayerId,
                Type = adminActionApiResponse.Result.Type,
                Text = adminActionApiResponse.Result.Text,
                Expires = adminActionApiResponse.Result.Expires,
                AdminId = adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId,
                PlayerDto = adminActionApiResponse.Result.PlayerDto
            };

            var canEditAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType, string>(adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId), AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAdminActionViewModel model)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(model.AdminActionId);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.PlayerDto = adminActionApiResponse.Result.PlayerDto;
                return View(model);
            }

            var canEditAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, AdminActionType, string>(adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId), AuthPolicies.EditAdminAction);

            if (!canEditAdminAction.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                Text = model.Text,
                Expires = model.Type == AdminActionType.TempBan ? model.Expires : null
            };

            var canChangeAdminActionAdmin = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.PlayerDto.GameType, AuthPolicies.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded)
                editAdminActionDto.AdminId = model.AdminId;

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            if (adminActionApiResponse.Result.ForumTopicId != 0)
                await adminActionTopics.UpdateTopicForAdminAction(adminActionApiResponse.Result.ForumTopicId, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.PlayerDto.PlayerId, adminActionApiResponse.Result.PlayerDto.Username, adminActionApiResponse.Result.Created, model.Text, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId);

            var eventTelemetry = new EventTelemetry("EditAdminAction").Enrich(User).Enrich(adminActionApiResponse.Result.PlayerDto).Enrich(editAdminActionDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The {model.Type} has been successfully updated for {adminActionApiResponse.Result.PlayerDto.Username}");

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Lift(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId), AuthPolicies.LiftAdminAction);

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

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, new Tuple<GameType, string>(adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId), AuthPolicies.LiftAdminAction);

            if (!canLiftAdminAction.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                Expires = DateTime.UtcNow
            };

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            if (adminActionApiResponse.Result.ForumTopicId != 0)
                await adminActionTopics.UpdateTopicForAdminAction(adminActionApiResponse.Result.ForumTopicId, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.PlayerDto.PlayerId, adminActionApiResponse.Result.PlayerDto.Username, adminActionApiResponse.Result.Created, adminActionApiResponse.Result.Text, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId);

            var eventTelemetry = new EventTelemetry("BanLifted").Enrich(User).Enrich(adminActionApiResponse.Result.PlayerDto).Enrich(editAdminActionDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The {adminActionApiResponse.Result.Type} has been successfully lifted for {adminActionApiResponse.Result.PlayerDto?.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> Claim(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.PlayerDto.GameType, AuthPolicies.ClaimAdminAction);

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

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.PlayerDto.GameType, AuthPolicies.ClaimAdminAction);

            if (!canClaimAdminAction.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                AdminId = User.XtremeIdiotsId()
            };

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            if (adminActionApiResponse.Result.ForumTopicId != 0)
                await adminActionTopics.UpdateTopicForAdminAction(adminActionApiResponse.Result.ForumTopicId, adminActionApiResponse.Result.Type, adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.PlayerDto.PlayerId, adminActionApiResponse.Result.PlayerDto.Username, adminActionApiResponse.Result.Created, adminActionApiResponse.Result.Text, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId);

            var eventTelemetry = new EventTelemetry("BanClaimed").Enrich(User).Enrich(adminActionApiResponse.Result.PlayerDto).Enrich(editAdminActionDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The {adminActionApiResponse.Result.Type} has been successfully claimed for {adminActionApiResponse.Result.PlayerDto?.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }

        [HttpGet]
        public async Task<IActionResult> CreateDiscussionTopic(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canCreateAdminActionDiscussionTopic = await authorizationService.AuthorizeAsync(User, adminActionApiResponse.Result.PlayerDto.GameType, AuthPolicies.CreateAdminActionTopic);

            if (!canCreateAdminActionDiscussionTopic.Succeeded)
                return Unauthorized();

            var editAdminActionDto = new EditAdminActionDto(adminActionApiResponse.Result.AdminActionId)
            {
                ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(adminActionApiResponse.Result.Type, adminActionApiResponse.Result.PlayerDto.GameType, adminActionApiResponse.Result.PlayerDto.PlayerId, adminActionApiResponse.Result.PlayerDto.Username, DateTime.UtcNow, adminActionApiResponse.Result.Text, adminActionApiResponse.Result.UserProfileDto.XtremeIdiotsForumId)
            };

            await repositoryApiClient.AdminActions.UpdateAdminAction(editAdminActionDto);

            var eventTelemetry = new EventTelemetry("CreateDiscussionTopic").Enrich(User).Enrich(adminActionApiResponse.Result.PlayerDto).Enrich(editAdminActionDto);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{adminActionApiResponse.Result.ForumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction("Details", "Players", new { id = adminActionApiResponse.Result.PlayerId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            return View(adminActionApiResponse.Result);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId)
        {
            var adminActionApiResponse = await repositoryApiClient.AdminActions.GetAdminAction(id);

            if (adminActionApiResponse.IsNotFound || adminActionApiResponse.Result == null || adminActionApiResponse.Result.PlayerDto == null || adminActionApiResponse.Result.UserProfileDto == null)
                return NotFound();

            var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteAdminAction);

            if (!canDeleteAdminAction.Succeeded)
                return Unauthorized();

            await repositoryApiClient.AdminActions.DeleteAdminAction(id);

            var eventTelemetry = new EventTelemetry("DeleteAdminAction").Enrich(User).Enrich(adminActionApiResponse.Result.PlayerDto).Enrich(adminActionApiResponse.Result);
            telemetryClient.TrackEvent(eventTelemetry);

            this.AddAlertSuccess($"The {adminActionApiResponse.Result.Type} has been successfully deleted from {adminActionApiResponse.Result.PlayerDto.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }
    }
}