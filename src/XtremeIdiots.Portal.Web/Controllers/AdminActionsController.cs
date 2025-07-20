using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
/// Controller for managing administrative actions against players
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
    /// Displays the form for creating a new admin action against a player
    /// </summary>
    /// <param name="id">The player ID to create an admin action against</param>
    /// <param name="adminActionType">The type of admin action to create</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Create admin action view with pre-populated data</returns>
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

            if (authResult is not null) return authResult;

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
    /// Processes the creation of a new admin action with form validation and authorization checks
    /// </summary>
    /// <param name="model">The create admin action view model containing form data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details on success, returns view with validation errors on failure</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate admin actions</exception>
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
            if (modelValidationResult is not null) return modelValidationResult;

            var authorizationResource = (playerData.GameType, model.Type);
            var authResult = await CheckAuthorizationAsync(
     authorizationService,
     authorizationResource,
     AuthPolicies.CreateAdminAction,
     "Create",
     "AdminAction",
     $"GameType:{playerData.GameType},AdminActionType:{model.Type}",
     playerData);

            if (authResult is not null) return authResult;

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
 { "ForumTopicId", createAdminActionDto.ForumTopicId?.ToString() ?? string.Empty }
        });

            this.AddAlertSuccess(CreateActionAppliedMessage(model.Type, playerData.Username, createAdminActionDto.ForumTopicId));

            return RedirectToAction("Details", "Players", new { id = model.PlayerId });
        }, $"CreateAdminAction-{model.Type}");
    }

    /// <summary>
    /// Displays the form for editing an existing admin action
    /// </summary>
    /// <param name="id">The admin action ID to edit</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>edit admin action view with current data, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionedit the admin action</exception>
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

            if (authResult is not null) return authResult;

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
    /// Processes the editing of an admin action with validation and authorization checks
    /// </summary>
    /// <param name="model">The edit admin action view model containing updated form data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details on success, returns view with validation errors on failure</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionedit admin actions</exception>
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
            if (modelValidationResult is not null) return modelValidationResult;

            var authorizationResource = (playerData.GameType, adminActionData.Type, adminActionData.UserProfile?.XtremeIdiotsForumId);
            var authResult = await CheckAuthorizationAsync(
     authorizationService,
     authorizationResource,
     AuthPolicies.EditAdminAction,
     "Edit",
     "AdminAction",
     $"GameType:{playerData.GameType},AdminActionId:{model.AdminActionId}",
     adminActionData);

            if (authResult is not null) return authResult;

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
    /// Displays the confirmation form for lifting (ending) an admin action
    /// </summary>
    /// <param name="id">The admin action ID to lift</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>lift confirmation view, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionlift admin actions</exception>
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

            if (authResult is not null) return authResult;

            return View(adminActionData);
        }, "LiftAdminActionForm");
    }

    /// <summary>
    /// Processes the lifting (ending) of an admin action by setting its expiry to now
    /// </summary>
    /// <param name="id">The admin action ID to lift</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details with success message, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionlift admin actions</exception>
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
     "AdminAction",
     $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}",
     adminActionData);

            if (authResult is not null) return authResult;

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
    /// Displays the confirmation form for claiming ownership of an unclaimed admin action
    /// </summary>
    /// <param name="id">The admin action ID to claim</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>claim confirmation view, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionclaim admin actions</exception>
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

            if (authResult is not null) return authResult;

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
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionclaim admin actions</exception>
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
     "AdminAction",
     $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}",
     adminActionData);

            if (authResult is not null) return authResult;

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
    /// Creates a forum discussion topic for an existing admin action that doesn't have one
    /// </summary>
    /// <param name="id">The admin action ID to create a discussion topic for</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details with success message and forum link, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate admin action topics</exception>
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

            if (authResult is not null) return authResult;

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
    /// Displays the confirmation form for deleting an admin action
    /// </summary>
    /// <param name="id">The admin action ID to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>delete confirmation view, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete admin actions</exception>
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

            if (authResult is not null) return authResult;

            return View(adminActionData);
        }, "DeleteAdminActionForm");
    }

    /// <summary>
    /// Processes the deletion of an admin action after confirmation
    /// </summary>
    /// <param name="id">The admin action ID to delete</param>
    /// <param name="playerId">The player ID associated with the admin action</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to player details with success message, or appropriate error response</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete admin actions</exception>
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
     "AdminAction",
     $"AdminActionId:{id},PlayerId:{playerId}",
     adminActionData);

            if (authResult is not null) return authResult;

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

    /// <summary>
    /// Gets the configured forum base URL or default
    /// </summary>
    /// <returns>The forum base URL for creating links to admin action topics</returns>
    private string GetForumBaseUrl() => GetConfigurationValue("AdminActions:ForumBaseUrl", DefaultForumBaseUrl);

    /// <summary>
    /// Gets the configured fallback admin ID or default
    /// </summary>
    /// <returns>The fallback admin ID to use when no admin is specified</returns>
    private string GetFallbackAdminId() => GetConfigurationValue("AdminActions:FallbackAdminId", DefaultFallbackAdminId);

    /// <summary>
    /// Retrieves player data by ID with error handling and validation
    /// </summary>
    /// <param name="playerId">The player ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The player data if found, null if not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when player data retrieval fails unexpectedly</exception>
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
    /// Retrieves admin action data by ID with error handling and validation
    /// </summary>
    /// <param name="adminActionId">The admin action ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The admin action data if found, null if not found or has no associated player</returns>
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
    /// Updates the forum topic for an admin action if it exists
    /// </summary>
    /// <param name="adminActionData">The admin action data containing forum topic information</param>
    /// <param name="text">The updated text for the admin action</param>
    /// <param name="adminForumId">The admin forum ID to associate with the action</param>
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
    /// Creates a success message for when an admin action is applied with forum topic link
    /// </summary>
    /// <param name="actionType">The type of admin action</param>
    /// <param name="username">The username of the affected player</param>
    /// <param name="forumTopicId">The forum topic ID for the admin action</param>
    /// <returns>A formatted HTML message with forum topic link</returns>
    private string CreateActionAppliedMessage(AdminActionType actionType, string username, int? forumTopicId)
    {
        var forumBaseUrl = GetForumBaseUrl();
        return $"The {actionType} has been successfully applied against {username} with a <a target=\"_blank\" href=\"{forumBaseUrl}{forumTopicId}-topic/\" class=\"alert-link\">topic</a>";
    }

    /// <summary>
    /// Creates a success message for admin action operations (updated, lifted, etc.)
    /// </summary>
    /// <param name="actionType">The type of admin action</param>
    /// <param name="username">The username of the affected player</param>
    /// <param name="operation">The operation performed (updated, lifted, etc.)</param>
    /// <returns>A formatted success message</returns>
    private static string CreateActionOperationMessage(AdminActionType actionType, string username, string operation) =>
    $"The {actionType} has been successfully {operation} for {username}";

    /// <summary>
    /// Displays admin actions created by the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>A view with the user's admin actions, or error page on failure</returns>
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
    /// Displays unclaimed admin actions (typically bans) that need to be assigned to administrators
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>A view with unclaimed admin actions, or error page on failure</returns>
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
}
