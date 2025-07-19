using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers;
/// <summary>
/// Controller for managing admin actions (bans, kicks, warnings, etc.) against players
/// </summary>
[Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
public class AdminActionsController : BaseController
{
    private const string DefaultForumBaseUrl = "https://www.xtremeidiots.com/forums/topic/";
    private const string DefaultFallbackAdminId = "21145";
    private const int DefaultTempBanDurationDays = 7;

    private readonly IAuthorizationService authorizationService;
    private readonly IAdminActionTopics adminActionTopics;
    private readonly IRepositoryApiClient repositoryApiClient;

    public AdminActionsController(
        IAuthorizationService authorizationService,
        IAdminActionTopics adminActionTopics,
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<AdminActionsController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.adminActionTopics = adminActionTopics ?? throw new ArgumentNullException(nameof(adminActionTopics));
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    /// <summary>
    /// Displays the creation form for a new admin action against a specific player
    /// </summary>
    /// <param name="id">The player ID to create an admin action for</param>
    /// <param name="adminActionType">The type of admin action to create (Ban, Kick, Warning, etc.)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The create admin action view with pre-populated form data, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when player is not found</exception>
    [HttpGet]
    public async Task<IActionResult> Create(Guid id, AdminActionType adminActionType, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var playerData = await GetPlayerDataAsync(id, cancellationToken);
            if (playerData is null)
                return NotFound();

            var authorizationResource = (playerData.GameType, adminActionType);

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.CreateAdminAction,
                "Create",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionType:{adminActionType}",
                playerData);

            if (authResult != null) return authResult;

            var createAdminActionViewModel = new CreateAdminActionViewModel
            {
                Type = adminActionType,
                PlayerId = playerData.PlayerId,
                PlayerDto = playerData,
                Expires = adminActionType == AdminActionType.TempBan ? DateTime.UtcNow.AddDays(DefaultTempBanDurationDays) : null
            };

            return View(createAdminActionViewModel);
        }, $"CreateAdminActionForm-{adminActionType}");
    }

    /// <summary>
    /// Creates a new admin action for a player based on the submitted form data
    /// </summary>
    /// <param name="model">The create admin action view model containing the action details</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details page on success, or returns the view with validation errors</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when player is not found</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAdminActionViewModel model, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var playerData = await GetPlayerDataAsync(model.PlayerId, cancellationToken);
            if (playerData is null)
                return NotFound();

            var modelValidationResult = CheckModelState(model, m => m.PlayerDto = playerData);
            if (modelValidationResult != null) return modelValidationResult;

            var authorizationResource = (playerData.GameType, model.Type);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.CreateAdminAction,
                "Create",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionType:{model.Type}",
                playerData);

            if (authResult != null) return authResult;

            var adminId = User.XtremeIdiotsId();
            var createAdminActionDto = new CreateAdminActionDto(playerData.PlayerId, model.Type, model.Text)
            {
                AdminId = adminId,
                Expires = model.Expires,
            };

            createAdminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(
                model.Type,
                playerData.GameType,
                playerData.PlayerId,
                playerData.Username,
                DateTime.UtcNow,
                model.Text,
                adminId);

            await repositoryApiClient.AdminActions.V1.CreateAdminAction(createAdminActionDto, cancellationToken);

            TrackSuccessTelemetry("AdminActionCreated", "CreateAdminAction", new Dictionary<string, string>
            {
                    { "PlayerId", model.PlayerId.ToString() },
                    { "AdminActionType", model.Type.ToString() },
                    { "ForumTopicId", createAdminActionDto.ForumTopicId?.ToString() ?? "null" }
            });

            var forumBaseUrl = GetForumBaseUrl();
            this.AddAlertSuccess(CreateActionAppliedMessage(model.Type, playerData.Username, createAdminActionDto.ForumTopicId));

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }, $"CreateAdminAction-{model.Type}");
    }

    /// <summary>
    /// Displays the edit form for modifying an existing admin action
    /// </summary>
    /// <param name="id">The admin action ID to edit</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The edit admin action view with populated form data, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to edit admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionData = await GetAdminActionDataAsync(id, cancellationToken);
            if (adminActionData is null)
                return NotFound();

            var playerData = adminActionData.Player!;

            var authorizationResource = (playerData.GameType, adminActionData.Type, adminActionData.UserProfile?.XtremeIdiotsForumId);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.EditAdminAction,
                "Edit",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            if (authResult != null) return authResult;

            var viewModel = new EditAdminActionViewModel
            {
                AdminActionId = adminActionData.AdminActionId,
                PlayerId = adminActionData.PlayerId,
                Type = adminActionData.Type,
                Text = adminActionData.Text,
                Expires = adminActionData.Expires,
                AdminId = adminActionData.UserProfile?.XtremeIdiotsForumId,
                PlayerDto = playerData
            };

            return View(viewModel);
        }, "EditAdminActionForm");
    }

    /// <summary>
    /// Updates an existing admin action with the provided modifications
    /// </summary>
    /// <param name="model">The edit admin action view model containing updated values</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details page on success, or returns the view with validation errors</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to edit admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditAdminActionViewModel model, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionData = await GetAdminActionDataAsync(model.AdminActionId, cancellationToken);
            if (adminActionData is null)
                return NotFound();

            var playerData = adminActionData.Player!;

            var modelValidationResult = CheckModelState(model, m => m.PlayerDto = playerData);
            if (modelValidationResult != null) return modelValidationResult;

            var authorizationResource = (playerData.GameType, adminActionData.Type, adminActionData.UserProfile?.XtremeIdiotsForumId);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.EditAdminAction,
                "Edit",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionId:{model.AdminActionId}",
                adminActionData);

            if (authResult != null) return authResult;

            var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
            {
                Text = model.Text,
                Expires = model.Type == AdminActionType.TempBan ? model.Expires : null
            };

            var canChangeAdminActionAdmin = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.ChangeAdminActionAdmin);

            if (canChangeAdminActionAdmin.Succeeded && adminActionData.UserProfile?.XtremeIdiotsForumId != model.AdminId)
            {
                editAdminActionDto.AdminId = string.IsNullOrWhiteSpace(model.AdminId) ? GetFallbackAdminId() : model.AdminId;
                Logger.LogInformation("User {UserId} changed admin for action {AdminActionId} to {NewAdminId}",
                    User.XtremeIdiotsId(), model.AdminActionId, editAdminActionDto.AdminId);
            }

            await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto, cancellationToken);

            var adminForumId = canChangeAdminActionAdmin.Succeeded && adminActionData.UserProfile?.XtremeIdiotsForumId != model.AdminId
                ? editAdminActionDto.AdminId
                : adminActionData.UserProfile?.XtremeIdiotsForumId;

            await UpdateForumTopicIfExistsAsync(adminActionData, model.Text, adminForumId);

            TrackSuccessTelemetry("AdminActionEdited", "EditAdminAction", new Dictionary<string, string>
            {
                    { "AdminActionId", model.AdminActionId.ToString() },
                    { "PlayerId", model.PlayerId.ToString() },
                    { "AdminActionType", model.Type.ToString() }
            });

            this.AddAlertSuccess(CreateActionOperationMessage(model.Type, playerData.Username, "updated"));

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }, "EditAdminAction");
    }

    /// <summary>
    /// Displays the lift confirmation page for an admin action
    /// </summary>
    /// <param name="id">The admin action ID to display lift confirmation for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The lift confirmation view with admin action details, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to lift admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpGet]
    public async Task<IActionResult> Lift(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionData = await GetAdminActionDataAsync(id, cancellationToken);
            if (adminActionData is null)
                return NotFound();

            var playerData = adminActionData.Player!;

            var authorizationResource = (playerData.GameType, adminActionData.UserProfile?.XtremeIdiotsForumId);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.LiftAdminAction,
                "Lift",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            if (authResult != null) return authResult;

            return View(adminActionData);
        }, "LiftAdminActionForm");
    }

    /// <summary>
    /// Lifts an admin action by setting its expiry date to the current time, effectively ending it
    /// </summary>
    /// <param name="id">The admin action ID to lift</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details page with success message, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to lift admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpPost]
    [ActionName("Lift")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LiftConfirmed(Guid id, Guid playerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionData = await GetAdminActionDataAsync(id, cancellationToken);
            if (adminActionData is null)
                return NotFound();

            var playerData = adminActionData.Player!;

            var authorizationResource = (playerData.GameType, adminActionData.UserProfile?.XtremeIdiotsForumId);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.LiftAdminAction,
                "Lift",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}",
                adminActionData);

            if (authResult != null) return authResult;

            var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
            {
                Expires = DateTime.UtcNow
            };

            await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto, cancellationToken);

            await UpdateForumTopicIfExistsAsync(adminActionData, adminActionData.Text, adminActionData.UserProfile?.XtremeIdiotsForumId);

            TrackSuccessTelemetry("AdminActionLifted", "LiftAdminAction", new Dictionary<string, string>
            {
                    { "AdminActionId", id.ToString() },
                    { "PlayerId", playerId.ToString() },
                    { "AdminActionType", adminActionData.Type.ToString() }
            });

            this.AddAlertSuccess(CreateActionOperationMessage(adminActionData.Type, playerData.Username, "lifted"));

            return RedirectToAction("Details", "Players", new { id = playerId });
        }, "LiftAdminAction");
    }

    /// <summary>
    /// Displays the claim confirmation page for an admin action
    /// </summary>
    /// <param name="id">The admin action ID to display claim confirmation for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The claim confirmation view with admin action details, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to claim admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpGet]
    public async Task<IActionResult> Claim(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);

            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for claim operation", id);
                return NotFound();
            }

            var adminActionData = getAdminActionResult.Result.Data;
            var playerData = adminActionData.Player;

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                playerData.GameType,
                AuthPolicies.ClaimAdminAction,
                "Claim",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            if (authResult != null) return authResult;

            return View(adminActionData);
        }, "ClaimAdminActionForm");
    }

    /// <summary>
    /// Claims ownership of an admin action by assigning it to the current user
    /// </summary>
    /// <param name="id">The admin action ID to claim</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details page with success message, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to claim admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpPost]
    [ActionName("Claim")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClaimConfirmed(Guid id, Guid playerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);

            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for claim operation", id);
                return NotFound();
            }

            var adminActionData = getAdminActionResult.Result.Data;
            var playerData = adminActionData.Player;

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                playerData.GameType,
                AuthPolicies.ClaimAdminAction,
                "Claim",
                "AdminAction",
                $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}",
                adminActionData);

            if (authResult != null) return authResult;

            var adminId = User.XtremeIdiotsId();
            var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
            {
                AdminId = adminId
            };

            await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto, cancellationToken);

            if (adminActionData.ForumTopicId.HasValue && adminActionData.ForumTopicId != 0)
            {
                await adminActionTopics.UpdateTopicForAdminAction(
                    adminActionData.ForumTopicId.Value,
                    adminActionData.Type,
                    playerData.GameType,
                    playerData.PlayerId,
                    playerData.Username,
                    adminActionData.Created,
                    adminActionData.Text,
                    adminId);
            }

            TrackSuccessTelemetry("AdminActionClaimed", "ClaimAdminAction", new Dictionary<string, string>
            {
                    { "AdminActionId", id.ToString() },
                    { "PlayerId", playerId.ToString() },
                    { "AdminActionType", adminActionData.Type.ToString() }
            });

            this.AddAlertSuccess($"The {adminActionData.Type} has been successfully claimed for {playerData.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }, "ClaimAdminAction");
    }

    /// <summary>
    /// Creates a discussion topic for an admin action in the forums
    /// </summary>
    /// <param name="id">The admin action ID to create a discussion topic for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details page with success message, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create discussion topics</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpGet]
    public async Task<IActionResult> CreateDiscussionTopic(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);

            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for discussion topic creation", id);
                return NotFound();
            }

            var adminActionData = getAdminActionResult.Result.Data;
            var playerData = adminActionData.Player;

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                playerData.GameType,
                AuthPolicies.CreateAdminActionTopic,
                "CreateDiscussionTopic",
                "AdminActionTopic",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            if (authResult != null) return authResult;

            var forumTopicId = await adminActionTopics.CreateTopicForAdminAction(
                adminActionData.Type,
                playerData.GameType,
                playerData.PlayerId,
                playerData.Username,
                DateTime.UtcNow,
                adminActionData.Text,
                adminActionData.UserProfile?.XtremeIdiotsForumId);

            var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
            {
                ForumTopicId = forumTopicId
            };

            await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto, cancellationToken);

            TrackSuccessTelemetry("AdminActionTopicCreated", "CreateDiscussionTopic", new Dictionary<string, string>
            {
                    { "AdminActionId", id.ToString() },
                    { "ForumTopicId", forumTopicId.ToString() },
                    { "PlayerId", adminActionData.PlayerId.ToString() }
            });

            var forumBaseUrl = GetForumBaseUrl();
            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"{forumBaseUrl}{forumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction("Details", "Players", new { id = adminActionData.PlayerId });
        }, "CreateDiscussionTopic");
    }

    /// <summary>
    /// Displays the delete confirmation page for an admin action
    /// </summary>
    /// <param name="id">The admin action ID to display deletion confirmation for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The delete confirmation view with admin action details, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);

            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for delete operation", id);
                return NotFound();
            }

            var adminActionData = getAdminActionResult.Result.Data;

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                adminActionData,
                AuthPolicies.DeleteAdminAction,
                "Delete",
                "AdminAction",
                $"AdminActionId:{id}",
                adminActionData);

            if (authResult != null) return authResult;

            return View(adminActionData);
        }, "DeleteAdminActionForm");
    }

    /// <summary>
    /// Permanently deletes an admin action from the system after confirmation
    /// </summary>
    /// <param name="id">The admin action ID to delete</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details page with success message, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete admin actions</exception>
    /// <exception cref="KeyNotFoundException">Thrown when admin action is not found</exception>
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, Guid playerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);

            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for delete operation", id);
                return NotFound();
            }

            var adminActionData = getAdminActionResult.Result.Data;
            var playerData = adminActionData.Player;

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                adminActionData,
                AuthPolicies.DeleteAdminAction,
                "Delete",
                "AdminAction",
                $"AdminActionId:{id},PlayerId:{playerId}",
                adminActionData);

            if (authResult != null) return authResult;

            await repositoryApiClient.AdminActions.V1.DeleteAdminAction(id, cancellationToken);

            TrackSuccessTelemetry("AdminActionDeleted", "DeleteAdminAction", new Dictionary<string, string>
            {
                    { "AdminActionId", id.ToString() },
                    { "PlayerId", playerId.ToString() },
                    { "AdminActionType", adminActionData.Type.ToString() }
            });

            this.AddAlertSuccess($"The {adminActionData.Type} has been successfully deleted from {playerData.Username}");

            return RedirectToAction("Details", "Players", new { id = playerId });
        }, "DeleteAdminAction");
    }

    /// <summary>
    /// Gets the forum base URL from configuration with fallback
    /// </summary>
    /// <returns>The forum base URL</returns>
    private string GetForumBaseUrl()
    {
        return GetConfigurationValue("AdminActions:ForumBaseUrl", DefaultForumBaseUrl);
    }

    /// <summary>
    /// Gets the fallback admin ID from configuration
    /// </summary>
    /// <returns>The fallback admin ID</returns>
    private string GetFallbackAdminId()
    {
        return GetConfigurationValue("AdminActions:FallbackAdminId", DefaultFallbackAdminId);
    }

    /// <summary>
    /// Retrieves player data with standardized error handling and logging
    /// </summary>
    /// <param name="playerId">The player ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Player data if found, null if not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when API call fails unexpectedly</exception>
    private async Task<PlayerDto?> GetPlayerDataAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var getPlayerResult = await repositoryApiClient.Players.V1.GetPlayer(playerId, PlayerEntityOptions.None);

        if (getPlayerResult.IsNotFound)
        {
            Logger.LogWarning("Player {PlayerId} not found", playerId);
            return null;
        }

        if (getPlayerResult.Result?.Data is null)
        {
            Logger.LogWarning("Player data is null for {PlayerId}", playerId);
            throw new InvalidOperationException($"Player data retrieval failed for ID: {playerId}");
        }

        return getPlayerResult.Result.Data;
    }

    /// <summary>
    /// Retrieves admin action data with standardized error handling and logging
    /// </summary>
    /// <param name="adminActionId">The admin action ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Admin action data if found, null if not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when API call fails unexpectedly</exception>
    private async Task<AdminActionDto?> GetAdminActionDataAsync(Guid adminActionId, CancellationToken cancellationToken = default)
    {
        var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(adminActionId, cancellationToken);

        if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
        {
            Logger.LogWarning("Admin action {AdminActionId} not found or has no associated player", adminActionId);
            return null;
        }

        return getAdminActionResult.Result.Data;
    }

    /// <summary>
    /// Updates forum topic for an admin action if it exists
    /// </summary>
    /// <param name="adminActionData">The admin action data</param>
    /// <param name="text">The updated text content</param>
    /// <param name="adminForumId">The admin forum ID to use</param>
    /// <returns>Task representing the async operation</returns>
    private async Task UpdateForumTopicIfExistsAsync(AdminActionDto adminActionData, string text, string? adminForumId)
    {
        if (adminActionData.ForumTopicId.HasValue && adminActionData.ForumTopicId != 0 && adminActionData.Player is not null)
        {
            await adminActionTopics.UpdateTopicForAdminAction(
                adminActionData.ForumTopicId.Value,
                adminActionData.Type,
                adminActionData.Player.GameType,
                adminActionData.Player.PlayerId,
                adminActionData.Player.Username,
                adminActionData.Created,
                text,
                adminForumId);
        }
    }

    /// <summary>
    /// Creates a success message for admin action creation with forum topic link
    /// </summary>
    /// <param name="actionType">The type of admin action</param>
    /// <param name="username">The username the action was applied to</param>
    /// <param name="forumTopicId">The forum topic ID</param>
    /// <returns>The formatted success message</returns>
    private string CreateActionAppliedMessage(AdminActionType actionType, string username, int? forumTopicId)
    {
        var forumBaseUrl = GetForumBaseUrl();
        return $"The {actionType} has been successfully applied against {username} with a <a target=\"_blank\" href=\"{forumBaseUrl}{forumTopicId}-topic/\" class=\"alert-link\">topic</a>";
    }

    /// <summary>
    /// Creates a success message for admin action updates
    /// </summary>
    /// <param name="actionType">The type of admin action</param>
    /// <param name="username">The username the action was applied to</param>
    /// <param name="operation">The operation performed (updated, lifted, claimed, etc.)</param>
    /// <returns>The formatted success message</returns>
    private static string CreateActionOperationMessage(AdminActionType actionType, string username, string operation)
    {
        return $"The {actionType} has been successfully {operation} for {username}";
    }

    /// <summary>
    /// Displays admin actions created by the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with the user's admin actions</returns>
    [HttpGet]
    public async Task<IActionResult> MyActions(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, User.XtremeIdiotsId(), null, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items == null)
            {
                Logger.LogWarning("Failed to retrieve admin actions for user {UserId}", User.XtremeIdiotsId());
                return RedirectToAction("Display", "Errors", new { id = 500 });
            }

            Logger.LogInformation("Successfully retrieved {Count} admin actions for user {UserId}",
                adminActionsApiResponse.Result.Data.Items.Count(), User.XtremeIdiotsId());

            return View(adminActionsApiResponse.Result.Data.Items);
        }, "MyActions");
    }

    /// <summary>
    /// Displays unclaimed ban admin actions that can be claimed by moderators
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The unclaimed admin actions view</returns>
    [HttpGet]
    public async Task<IActionResult> Unclaimed(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, null, AdminActionFilter.UnclaimedBans, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items == null)
            {
                Logger.LogWarning("Failed to retrieve unclaimed admin actions for user {UserId}", User.XtremeIdiotsId());
                return RedirectToAction("Display", "Errors", new { id = 500 });
            }

            Logger.LogInformation("Successfully retrieved {Count} unclaimed admin actions for user {UserId}",
                adminActionsApiResponse.Result.Data.Items.Count(), User.XtremeIdiotsId());

            return View(adminActionsApiResponse.Result.Data.Items);
        }, "Unclaimed");
    }
}
