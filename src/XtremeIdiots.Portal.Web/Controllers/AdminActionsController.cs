using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing admin actions against players in the gaming portal
/// </summary>
[Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
public class AdminActionsController(
    IAuthorizationService authorizationService,
    IAdminActionTopics adminActionTopics,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<AdminActionsController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private const string DefaultForumBaseUrl = "https://www.xtremeidiots.com/forums/topic/";
    private const string DefaultFallbackAdminId = "21145";
    private const int DefaultTempBanDurationDays = 7;

    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly IAdminActionTopics adminActionTopics = adminActionTopics ?? throw new ArgumentNullException(nameof(adminActionTopics));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the create admin action form for a specific player
    /// </summary>
    /// <param name="id">The player ID</param>
    /// <param name="adminActionType">The type of admin action to create</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The create admin action view</returns>
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
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionType:{adminActionType}",
                playerData);

            if (authResult is not null)
                return authResult;

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
    /// Creates a new admin action for the specified player
    /// </summary>
    /// <param name="model">The create admin action view model containing form data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details on success, returns view with validation errors on failure</returns>
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
            if (modelValidationResult is not null)
                return modelValidationResult;

            var authorizationResource = (playerData.GameType, model.Type);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.CreateAdminAction,
                "Create",
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionType:{model.Type}",
                playerData);

            if (authResult is not null)
                return authResult;

            var adminId = User.XtremeIdiotsId();
            var createAdminActionDto = new CreateAdminActionDto(playerData.PlayerId, model.Type, model.Text)
            {
                AdminId = adminId,
                Expires = model.Expires,
                ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(
                    model.Type,
                    playerData.GameType,
                    playerData.PlayerId,
                    playerData.Username,
                    DateTime.UtcNow,
                    model.Text,
                    adminId)
            };

            await repositoryApiClient.AdminActions.V1.CreateAdminAction(createAdminActionDto, cancellationToken);

            TrackSuccessTelemetry("AdminActionCreated", "CreateAdminAction", new Dictionary<string, string>
            {
                { "PlayerId", model.PlayerId.ToString() },
                { "AdminActionType", model.Type.ToString() },
                { "ForumTopicId", createAdminActionDto.ForumTopicId?.ToString() ?? string.Empty }
            });

            this.AddAlertSuccess(CreateActionAppliedMessage(model.Type, playerData.Username, createAdminActionDto.ForumTopicId));

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }, $"CreateAdminAction-{model.Type}");
    }

    /// <summary>
    /// Displays the edit admin action form
    /// </summary>
    /// <param name="id">The admin action ID to edit</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The edit admin action view</returns>
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
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            if (authResult is not null)
                return authResult;

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
    /// Updates an existing admin action with new information
    /// </summary>
    /// <param name="model">The edit admin action view model containing updated data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details on success, returns view with validation errors on failure</returns>
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
            if (modelValidationResult is not null)
                return modelValidationResult;

            var authorizationResource = (playerData.GameType, adminActionData.Type, adminActionData.UserProfile?.XtremeIdiotsForumId);
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                authorizationResource,
                AuthPolicies.EditAdminAction,
                "Edit",
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionId:{model.AdminActionId}",
                adminActionData);

            if (authResult is not null)
                return authResult;

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

            TrackSuccessTelemetry("AdminActionEdited", nameof(Edit), new Dictionary<string, string>
            {
                { nameof(model.AdminActionId), model.AdminActionId.ToString() },
                { nameof(model.PlayerId), model.PlayerId.ToString() },
                { "AdminActionType", model.Type.ToString() }
            });

            this.AddAlertSuccess(CreateActionOperationMessage(model.Type, playerData.Username, "updated"));

            return RedirectToAction(nameof(PlayersController.Details), "Players", new { id = model.PlayerId });
        }, nameof(Edit));
    }

    /// <summary>
    /// Displays the lift admin action confirmation form
    /// </summary>
    /// <param name="id">The admin action ID to lift</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The lift confirmation view</returns>
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
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            return authResult is not null ? authResult : View(adminActionData);
        }, "LiftAdminActionForm");
    }

    /// <summary>
    /// Lifts (expires) an admin action immediately
    /// </summary>
    /// <param name="id">The admin action ID to lift</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details after lifting the action</returns>
    [HttpPost]
    [ActionName(nameof(Lift))]
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
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}",
                adminActionData);

            if (authResult is not null)
                return authResult;

            var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
            {
                Expires = DateTime.UtcNow
            };

            await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto, cancellationToken);

            await UpdateForumTopicIfExistsAsync(adminActionData, adminActionData.Text, adminActionData.UserProfile?.XtremeIdiotsForumId);

            TrackSuccessTelemetry("AdminActionLifted", nameof(Lift), new Dictionary<string, string>
            {
                { "AdminActionId", id.ToString() },
                { nameof(playerId), playerId.ToString() },
                { "AdminActionType", adminActionData.Type.ToString() }
            });

            this.AddAlertSuccess(CreateActionOperationMessage(adminActionData.Type, playerData.Username, "lifted"));

            return RedirectToAction(nameof(PlayersController.Details), "Players", new { id = playerId });
        }, nameof(Lift));
    }

    /// <summary>
    /// Displays the claim admin action confirmation form
    /// </summary>
    /// <param name="id">The admin action ID to claim</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The claim confirmation view</returns>
    [HttpGet]
    public async Task<IActionResult> Claim(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);

            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for claim operation", id);
                return NotFound();
            }

            var adminActionData = getAdminActionResult.Result.Data;

            // Ensure player data is available (some API responses may omit nested player details)
            var playerData = adminActionData.Player;
            if (playerData is null)
            {
                var playerResult = await repositoryApiClient.Players.V1.GetPlayer(adminActionData.PlayerId, PlayerEntityOptions.None);
                if (playerResult.IsNotFound || playerResult.Result?.Data is null)
                {
                    Logger.LogWarning("Player {PlayerId} not found when enriching admin action {AdminActionId} for claim operation", adminActionData.PlayerId, id);
                    return NotFound();
                }
                playerData = playerResult.Result.Data;
            }

            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                playerData.GameType,
                AuthPolicies.ClaimAdminAction,
                "Claim",
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionId:{id}",
                adminActionData);

            return authResult is not null ? authResult : View(adminActionData);
        }, "ClaimAdminActionForm");
    }

    /// <summary>
    /// Claims an admin action for the current user
    /// </summary>
    /// <param name="id">The admin action ID to claim</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details after claiming the action</returns>
    [HttpPost]
    [ActionName(nameof(Claim))]
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
                "AdminActions",
                $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}",
                adminActionData);

            if (authResult is not null)
                return authResult;

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

            TrackSuccessTelemetry("AdminActionClaimed", nameof(Claim), new Dictionary<string, string>
            {
                { "AdminActionId", id.ToString() },
                { nameof(playerId), playerId.ToString() },
                { "AdminActionType", adminActionData.Type.ToString() }
            });

            this.AddAlertSuccess($"The {adminActionData.Type} has been successfully claimed for {playerData.Username}");

            return RedirectToAction(nameof(PlayersController.Details), "Players", new { id = playerId });
        }, nameof(Claim));
    }

    /// <summary>
    /// Creates a forum discussion topic for an existing admin action
    /// </summary>
    /// <param name="id">The admin action ID to create a topic for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details after creating the topic</returns>
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

            if (authResult is not null)
                return authResult;

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

            TrackSuccessTelemetry("AdminActionTopicCreated", nameof(CreateDiscussionTopic), new Dictionary<string, string>
            {
                { "AdminActionId", id.ToString() },
                { "ForumTopicId", forumTopicId.ToString() },
                { nameof(adminActionData.PlayerId), adminActionData.PlayerId.ToString() }
            });

            var forumBaseUrl = GetForumBaseUrl();
            this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"{forumBaseUrl}{forumTopicId}-topic/\" class=\"alert-link\">here</a>");

            return RedirectToAction(nameof(PlayersController.Details), "Players", new { id = adminActionData.PlayerId });
        }, nameof(CreateDiscussionTopic));
    }

    /// <summary>
    /// Displays the delete admin action confirmation form
    /// </summary>
    /// <param name="id">The admin action ID to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The delete confirmation view</returns>
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
                "AdminActions",
                $"AdminActionId:{id}",
                adminActionData);

            return authResult is not null ? authResult : View(adminActionData);
        }, "DeleteAdminActionForm");
    }

    /// <summary>
    /// Permanently deletes an admin action
    /// </summary>
    /// <param name="id">The admin action ID to delete</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details after deleting the action</returns>
    [HttpPost]
    [ActionName(nameof(Delete))]
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
                "AdminActions",
                $"AdminActionId:{id},PlayerId:{playerId}",
                adminActionData);

            if (authResult is not null)
                return authResult;

            await repositoryApiClient.AdminActions.V1.DeleteAdminAction(id, cancellationToken);

            TrackSuccessTelemetry("AdminActionDeleted", nameof(Delete), new Dictionary<string, string>
            {
                { "AdminActionId", id.ToString() },
                { nameof(playerId), playerId.ToString() },
                { "AdminActionType", adminActionData.Type.ToString() }
            });

            this.AddAlertSuccess($"The {adminActionData.Type} has been successfully deleted from {playerData.Username}");

            return RedirectToAction(nameof(PlayersController.Details), "Players", new { id = playerId });
        }, nameof(Delete));
    }

    private string GetForumBaseUrl()
    {
        return GetConfigurationValue("AdminActions:ForumBaseUrl", DefaultForumBaseUrl);
    }

    private string GetFallbackAdminId()
    {
        return GetConfigurationValue("AdminActions:FallbackAdminId", DefaultFallbackAdminId);
    }

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

    private string CreateActionAppliedMessage(AdminActionType actionType, string username, int? forumTopicId)
    {
        var forumBaseUrl = GetForumBaseUrl();
        return $"The {actionType} has been successfully applied against {username} with a <a target=\"_blank\" href=\"{forumBaseUrl}{forumTopicId}-topic/\" class=\"alert-link\">topic</a>";
    }

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

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve admin actions for user {UserId}", User.XtremeIdiotsId());
                return RedirectToAction("Display", "Errors", new { id = 500 });
            }

            Logger.LogInformation("Successfully retrieved {Count} admin actions for user {UserId}",
                adminActionsApiResponse.Result.Data.Items.Count(), User.XtremeIdiotsId());

            return View(adminActionsApiResponse.Result.Data.Items);
        }, nameof(MyActions));
    }

    /// <summary>
    /// Displays unclaimed admin actions (bans without assigned administrators)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with unclaimed admin actions</returns>
    [HttpGet]
    public async Task<IActionResult> Unclaimed(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(null, null, null, AdminActionFilter.UnclaimedBans, 0, 50, AdminActionOrder.CreatedDesc);

            if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve unclaimed admin actions for user {UserId}", User.XtremeIdiotsId());
                return RedirectToAction("Display", "Errors", new { id = 500 });
            }

            Logger.LogInformation("Successfully retrieved {Count} unclaimed admin actions for user {UserId}",
                adminActionsApiResponse.Result.Data.Items.Count(), User.XtremeIdiotsId());

            return View(adminActionsApiResponse.Result.Data.Items);
        }, nameof(Unclaimed));
    }

    /// <summary>
    /// Displays a global list of admin actions with client-side filtering and sorting capabilities.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View containing a table of recent admin actions</returns>
    [HttpGet]
    public async Task<IActionResult> Global(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            // Fetch a larger page to allow client-side filtering/search (pagination can be added later if required)
            // Now using server-side pagination via AJAX (DataTables); initial page does not need data.
            return View();
        }, nameof(Global));
    }

    /// <summary>
    /// Provides server-side paginated admin actions for DataTables AJAX endpoint.
    /// Supports filtering by game type and admin action type. Ordering currently by Created only.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON result for DataTables</returns>
    [HttpPost]
    public async Task<IActionResult> GetAdminActionsAjax(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.DataTableAjaxPostModel>(requestBody);
            if (model is null)
            {
                Logger.LogWarning("Invalid DataTable model for admin actions by user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

            // Extract optional custom filters passed via additional POST data (DataTables 'ajax.data' lambda)
            GameType? gameType = null;
            AdminActionFilter? apiFilter = null;
            string? adminId = null;
            if (Request.Query.TryGetValue("gameType", out var gameTypeValues) && Enum.TryParse<GameType>(gameTypeValues.FirstOrDefault(), out var gt))
                gameType = gt;
            if (Request.Query.TryGetValue("adminActionFilter", out var filterValues) && Enum.TryParse<AdminActionFilter>(filterValues.FirstOrDefault(), out var f))
                apiFilter = f;
            if (Request.Query.TryGetValue("adminId", out var adminIdValues))
                adminId = adminIdValues.FirstOrDefault();

            var order = AdminActionOrder.CreatedDesc;
            if (model.Order?.Count > 0)
            {
                var dir = model.Order.First().Dir;
                // Attempt to use CreatedAsc if available when user sorts ascending on Created column (index 0)
                if (model.Order.First().Column == 0 && dir == "asc")
                {
                    try
                    {
                        order = AdminActionOrder.CreatedAsc;
                    }
                    catch
                    {
                        order = AdminActionOrder.CreatedDesc;
                    }
                }
            }
            // API currently does not expose admin action type filtering; fetch raw page
            var apiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                gameType, null, adminId, apiFilter, model.Start, model.Length, order, cancellationToken);

            if (!apiResponse.IsSuccess || apiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve admin actions list for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500);
            }

            var items = apiResponse.Result.Data.Items.ToList();

            return Json(new
            {
                model.Draw,
                recordsTotal = apiResponse.Result.Pagination?.TotalCount,
                recordsFiltered = apiResponse.Result.Pagination?.FilteredCount,
                data = items.Select(a => new
                {
                    created = a.Created.ToString("yyyy-MM-dd HH:mm"),
                    gameType = a.Player?.GameType.ToString(),
                    type = a.Type.ToString(),
                    player = a.Player?.Username,
                    playerId = a.PlayerId,
                    guid = a.Player?.Guid,
                    admin = a.UserProfile?.DisplayName ?? "Unclaimed",
                    expires = a.Expires?.ToString("yyyy-MM-dd HH:mm") ?? (a.Type == AdminActionType.Ban ? "Never" : string.Empty)
                })
            });
        }, nameof(GetAdminActionsAjax));
    }

    /// <summary>
    /// Provides server-side paginated admin actions for the currently logged in admin ("My Actions")
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON result for DataTables</returns>
    [HttpPost]
    public async Task<IActionResult> GetMyAdminActionsAjax(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync(cancellationToken);

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.DataTableAjaxPostModel>(requestBody);
            if (model is null)
            {
                Logger.LogWarning("Invalid DataTable model for my admin actions by user {UserId}", User.XtremeIdiotsId());
                return BadRequest();
            }

            GameType? gameType = null;
            AdminActionFilter? apiFilter = null;
            if (Request.Query.TryGetValue("gameType", out var gameTypeValues) && Enum.TryParse<GameType>(gameTypeValues.FirstOrDefault(), out var gt))
                gameType = gt;
            if (Request.Query.TryGetValue("adminActionFilter", out var filterValues) && Enum.TryParse<AdminActionFilter>(filterValues.FirstOrDefault(), out var f))
                apiFilter = f;

            // Always constrain to current user
            var adminId = User.XtremeIdiotsId();

            var order = AdminActionOrder.CreatedDesc;
            if (model.Order?.Count > 0)
            {
                var dir = model.Order.First().Dir;
                if (model.Order.First().Column == 0 && dir == "asc")
                {
                    try
                    {
                        order = AdminActionOrder.CreatedAsc;
                    }
                    catch
                    {
                        order = AdminActionOrder.CreatedDesc;
                    }
                }
            }

            var apiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                gameType, null, adminId, apiFilter, model.Start, model.Length, order, cancellationToken);

            if (!apiResponse.IsSuccess || apiResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve my admin actions list for user {UserId}", User.XtremeIdiotsId());
                return StatusCode(500);
            }

            var items = apiResponse.Result.Data.Items.ToList();

            return Json(new
            {
                model.Draw,
                recordsTotal = apiResponse.Result.Pagination?.TotalCount,
                recordsFiltered = apiResponse.Result.Pagination?.FilteredCount,
                data = items.Select(a => new
                {
                    created = a.Created.ToString("yyyy-MM-dd HH:mm"),
                    gameType = a.Player?.GameType.ToString(),
                    type = a.Type.ToString(),
                    player = a.Player?.Username,
                    playerId = a.PlayerId,
                    guid = a.Player?.Guid,
                    expires = a.Expires?.ToString("yyyy-MM-dd HH:mm") ?? (a.Type == AdminActionType.Ban ? "Never" : string.Empty),
                    id = a.AdminActionId,
                    text = a.Text
                })
            });
        }, nameof(GetMyAdminActionsAjax));
    }

    /// <summary>
    /// Returns a partial view with full details for an admin action (used in My Actions details panel)
    /// </summary>
    /// <param name="id">Admin action id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Partial HTML</returns>
    [HttpGet]
    public async Task<IActionResult> GetMyAdminActionDetails(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id, cancellationToken);
            if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data is null)
            {
                Logger.LogWarning("Admin action {AdminActionId} not found for my details panel", id);
                return NotFound();
            }

            var adminAction = getAdminActionResult.Result.Data;
            PlayerDto? player = null;

            var playerResult = await repositoryApiClient.Players.V1.GetPlayer(adminAction.PlayerId, PlayerEntityOptions.None);
            if (!playerResult.IsNotFound && playerResult.Result?.Data is not null)
            {
                player = playerResult.Result.Data;
            }

            var vm = new MyAdminActionDetailsViewModel(adminAction, player);
            return PartialView("_MyAdminActionDetailsPanelMy", vm);
        }, nameof(GetMyAdminActionDetails));
    }

    // Recent admin actions feature removed (was action: Recent) as per maintenance decision.
}