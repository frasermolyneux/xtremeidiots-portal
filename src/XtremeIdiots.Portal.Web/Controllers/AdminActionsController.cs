using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.AdminActions;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing admin actions (bans, kicks, warnings, etc.)
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessAdminActionsController)]
    public class AdminActionsController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IAdminActionTopics adminActionTopics;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<AdminActionsController> logger;

        public AdminActionsController(
            IAuthorizationService authorizationService,
            IAdminActionTopics adminActionTopics,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<AdminActionsController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.adminActionTopics = adminActionTopics ?? throw new ArgumentNullException(nameof(adminActionTopics));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                logger.LogInformation("User {UserId} attempting to create {AdminActionType} for player {PlayerId}",
                    User.XtremeIdiotsId(), adminActionType, id);

                var getPlayerResult = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (getPlayerResult.IsNotFound)
                {
                    logger.LogWarning("Player {PlayerId} not found when creating admin action", id);
                    return NotFound();
                }

                if (getPlayerResult.Result?.Data is null)
                {
                    logger.LogWarning("Player data is null for {PlayerId}", id);
                    return BadRequest();
                }

                var playerData = getPlayerResult.Result.Data;
                var authorizationResource = (playerData.GameType, adminActionType);
                var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.CreateAdminAction);

                if (!canCreateAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create {AdminActionType} for player {PlayerId} in game {GameType}",
                        User.XtremeIdiotsId(), adminActionType, id, playerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(playerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionType:{adminActionType}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var createAdminActionViewModel = new CreateAdminActionViewModel
                {
                    Type = adminActionType,
                    PlayerId = playerData.PlayerId,
                    PlayerDto = playerData,
                    Expires = adminActionType == AdminActionType.TempBan ? DateTime.UtcNow.AddDays(7) : null
                };

                logger.LogInformation("Successfully loaded create admin action form for user {UserId} targeting player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                return View(createAdminActionViewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating admin action form for player {PlayerId} and action type {AdminActionType}",
                    id, adminActionType);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", id.ToString());
                errorTelemetry.Properties.TryAdd("AdminActionType", adminActionType.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to create {AdminActionType} for player {PlayerId}",
                    User.XtremeIdiotsId(), model.Type, model.PlayerId);

                var getPlayerResult = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);

                if (getPlayerResult.IsNotFound || getPlayerResult.Result?.Data is null)
                {
                    logger.LogWarning("Player {PlayerId} not found when creating admin action", model.PlayerId);
                    return NotFound();
                }

                var playerData = getPlayerResult.Result.Data;

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for creating admin action for player {PlayerId}", model.PlayerId);
                    model.PlayerDto = playerData;
                    return View(model);
                }

                var authorizationResource = (playerData.GameType, model.Type);
                var canCreateAdminAction = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.CreateAdminAction);

                if (!canCreateAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create {AdminActionType} for player {PlayerId} in game {GameType}",
                        User.XtremeIdiotsId(), model.Type, model.PlayerId, playerData.GameType);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(playerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionType:{model.Type}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var adminId = User.XtremeIdiotsId();
                var createAdminActionDto = new CreateAdminActionDto(playerData.PlayerId, model.Type, model.Text)
                {
                    AdminId = adminId,
                    Expires = model.Expires,
                };

                // Create forum topic for admin action
                createAdminActionDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(
                    model.Type,
                    playerData.GameType,
                    playerData.PlayerId,
                    playerData.Username,
                    DateTime.UtcNow,
                    model.Text,
                    adminId);

                await repositoryApiClient.AdminActions.V1.CreateAdminAction(createAdminActionDto);

                var eventTelemetry = new EventTelemetry("AdminActionCreated")
                    .Enrich(User)
                    .Enrich(playerData)
                    .Enrich(createAdminActionDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully created {AdminActionType} for player {PlayerId} with forum topic {ForumTopicId}",
                    User.XtremeIdiotsId(), model.Type, model.PlayerId, createAdminActionDto.ForumTopicId);

                this.AddAlertSuccess($"The {model.Type} has been successfully applied against {playerData.Username} with a <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{createAdminActionDto.ForumTopicId}-topic/\" class=\"alert-link\">topic</a>");

                return RedirectToAction("Details", "Players", new { id = model.PlayerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating admin action for player {PlayerId} and action type {AdminActionType}",
                    model.PlayerId, model.Type);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("PlayerId", model.PlayerId.ToString());
                errorTelemetry.Properties.TryAdd("AdminActionType", model.Type.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while creating the admin action. Please try again.");

                // Reload player data for view
                var getPlayerResult = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                model.PlayerDto = getPlayerResult.Result?.Data;

                return View(model);
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to edit admin action {AdminActionId}",
                    User.XtremeIdiotsId(), id);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var authorizationResource = (playerData.GameType, adminActionData.Type, adminActionData.UserProfile?.XtremeIdiotsForumId);
                var canEditAdminAction = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.EditAdminAction);

                if (!canEditAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

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

                logger.LogInformation("Successfully loaded edit admin action form for admin action {AdminActionId}", id);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading edit form for admin action {AdminActionId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to update admin action {AdminActionId}",
                    User.XtremeIdiotsId(), model.AdminActionId);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(model.AdminActionId);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found during update", model.AdminActionId);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for updating admin action {AdminActionId}", model.AdminActionId);
                    model.PlayerDto = playerData;
                    return View(model);
                }

                var authorizationResource = (playerData.GameType, adminActionData.Type, adminActionData.UserProfile?.XtremeIdiotsForumId);
                var canEditAdminAction = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.EditAdminAction);

                if (!canEditAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit admin action {AdminActionId}",
                        User.XtremeIdiotsId(), model.AdminActionId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{model.AdminActionId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
                {
                    Text = model.Text,
                    Expires = model.Type == AdminActionType.TempBan ? model.Expires : null
                };

                var canChangeAdminActionAdmin = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.ChangeAdminActionAdmin);

                if (canChangeAdminActionAdmin.Succeeded && adminActionData.UserProfile?.XtremeIdiotsForumId != model.AdminId)
                {
                    editAdminActionDto.AdminId = string.IsNullOrWhiteSpace(model.AdminId) ? "21145" : model.AdminId; // Admin fallback
                    logger.LogInformation("User {UserId} changed admin for action {AdminActionId} to {NewAdminId}",
                        User.XtremeIdiotsId(), model.AdminActionId, editAdminActionDto.AdminId);
                }

                await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto);

                // Update forum topic if it exists
                if (adminActionData.ForumTopicId.HasValue && adminActionData.ForumTopicId != 0)
                {
                    var adminForumId = canChangeAdminActionAdmin.Succeeded && adminActionData.UserProfile?.XtremeIdiotsForumId != model.AdminId
                        ? editAdminActionDto.AdminId
                        : adminActionData.UserProfile?.XtremeIdiotsForumId;

                    await adminActionTopics.UpdateTopicForAdminAction(
                        adminActionData.ForumTopicId.Value,
                        adminActionData.Type,
                        playerData.GameType,
                        playerData.PlayerId,
                        playerData.Username,
                        adminActionData.Created,
                        model.Text,
                        adminForumId);
                }

                var eventTelemetry = new EventTelemetry("AdminActionEdited")
                    .Enrich(User)
                    .Enrich(playerData)
                    .Enrich(editAdminActionDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully updated admin action {AdminActionId}",
                    User.XtremeIdiotsId(), model.AdminActionId);

                this.AddAlertSuccess($"The {model.Type} has been successfully updated for {playerData.Username}");

                return RedirectToAction("Details", "Players", new { id = model.PlayerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating admin action {AdminActionId}", model.AdminActionId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", model.AdminActionId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while updating the admin action. Please try again.");

                // Reload admin action data for view
                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(model.AdminActionId);
                model.PlayerDto = getAdminActionResult.Result?.Data?.Player;

                return View(model);
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view lift confirmation for admin action {AdminActionId}",
                    User.XtremeIdiotsId(), id);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for lift operation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var authorizationResource = (playerData.GameType, adminActionData.UserProfile?.XtremeIdiotsForumId);
                var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.LiftAdminAction);

                if (!canLiftAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to lift admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Lift");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                return View(adminActionData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading lift confirmation for admin action {AdminActionId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to lift admin action {AdminActionId} for player {PlayerId}",
                    User.XtremeIdiotsId(), id, playerId);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for lift operation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var authorizationResource = (playerData.GameType, adminActionData.UserProfile?.XtremeIdiotsForumId);
                var canLiftAdminAction = await authorizationService.AuthorizeAsync(User, authorizationResource, AuthPolicies.LiftAdminAction);

                if (!canLiftAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to lift admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Lift");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{id},PlayerId:{playerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
                {
                    Expires = DateTime.UtcNow
                };

                await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto);

                // Update forum topic if it exists
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
                        adminActionData.UserProfile?.XtremeIdiotsForumId);
                }

                var eventTelemetry = new EventTelemetry("AdminActionLifted")
                    .Enrich(User)
                    .Enrich(playerData)
                    .Enrich(editAdminActionDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully lifted admin action {AdminActionId} for player {PlayerId}",
                    User.XtremeIdiotsId(), id, playerId);

                this.AddAlertSuccess($"The {adminActionData.Type} has been successfully lifted for {playerData.Username}");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error lifting admin action {AdminActionId} for player {PlayerId}", id, playerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                errorTelemetry.Properties.TryAdd("PlayerId", playerId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while lifting the admin action. Please try again.");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view claim confirmation for admin action {AdminActionId}",
                    User.XtremeIdiotsId(), id);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for claim operation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.ClaimAdminAction);

                if (!canClaimAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to claim admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Claim");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                return View(adminActionData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading claim confirmation for admin action {AdminActionId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to claim admin action {AdminActionId} for player {PlayerId}",
                    User.XtremeIdiotsId(), id, playerId);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for claim operation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var canClaimAdminAction = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.ClaimAdminAction);

                if (!canClaimAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to claim admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Claim");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var adminId = User.XtremeIdiotsId();
                var editAdminActionDto = new EditAdminActionDto(adminActionData.AdminActionId)
                {
                    AdminId = adminId
                };

                await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto);

                // Update forum topic if it exists
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

                var eventTelemetry = new EventTelemetry("AdminActionClaimed")
                    .Enrich(User)
                    .Enrich(playerData)
                    .Enrich(editAdminActionDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully claimed admin action {AdminActionId} for player {PlayerId}",
                    User.XtremeIdiotsId(), id, playerId);

                this.AddAlertSuccess($"The {adminActionData.Type} has been successfully claimed for {playerData.Username}");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error claiming admin action {AdminActionId} for player {PlayerId}", id, playerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                errorTelemetry.Properties.TryAdd("PlayerId", playerId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while claiming the admin action. Please try again.");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to create discussion topic for admin action {AdminActionId}",
                    User.XtremeIdiotsId(), id);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for discussion topic creation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var canCreateAdminActionDiscussionTopic = await authorizationService.AuthorizeAsync(User, playerData.GameType, AuthPolicies.CreateAdminActionTopic);

                if (!canCreateAdminActionDiscussionTopic.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create discussion topic for admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "CreateDiscussionTopic");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminActionTopic");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

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

                await repositoryApiClient.AdminActions.V1.UpdateAdminAction(editAdminActionDto);

                var eventTelemetry = new EventTelemetry("AdminActionTopicCreated")
                    .Enrich(User)
                    .Enrich(playerData)
                    .Enrich(editAdminActionDto);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully created discussion topic {ForumTopicId} for admin action {AdminActionId}",
                    User.XtremeIdiotsId(), forumTopicId, id);

                this.AddAlertSuccess($"The discussion topic has been successfully created <a target=\"_blank\" href=\"https://www.xtremeidiots.com/forums/topic/{forumTopicId}-topic/\" class=\"alert-link\">here</a>");

                return RedirectToAction("Details", "Players", new { id = adminActionData.PlayerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating discussion topic for admin action {AdminActionId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while creating the discussion topic. Please try again.");

                // Try to get player ID for redirect
                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);
                var playerId = getAdminActionResult.Result?.Data?.PlayerId ?? Guid.Empty;

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to view delete confirmation for admin action {AdminActionId}",
                    User.XtremeIdiotsId(), id);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for delete operation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;

                var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteAdminAction);

                if (!canDeleteAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"AdminActionId:{id}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                return View(adminActionData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading delete confirmation for admin action {AdminActionId}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
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
            try
            {
                logger.LogInformation("User {UserId} attempting to delete admin action {AdminActionId} for player {PlayerId}",
                    User.XtremeIdiotsId(), id, playerId);

                var getAdminActionResult = await repositoryApiClient.AdminActions.V1.GetAdminAction(id);

                if (getAdminActionResult.IsNotFound || getAdminActionResult.Result?.Data?.Player is null)
                {
                    logger.LogWarning("Admin action {AdminActionId} not found for delete operation", id);
                    return NotFound();
                }

                var adminActionData = getAdminActionResult.Result.Data;
                var playerData = adminActionData.Player;

                var canDeleteAdminAction = await authorizationService.AuthorizeAsync(User, AuthPolicies.DeleteAdminAction);

                if (!canDeleteAdminAction.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete admin action {AdminActionId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(adminActionData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"AdminActionId:{id},PlayerId:{playerId}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                await repositoryApiClient.AdminActions.V1.DeleteAdminAction(id);

                var eventTelemetry = new EventTelemetry("AdminActionDeleted")
                    .Enrich(User)
                    .Enrich(playerData)
                    .Enrich(adminActionData);
                telemetryClient.TrackEvent(eventTelemetry);

                logger.LogInformation("User {UserId} successfully deleted admin action {AdminActionId} for player {PlayerId}",
                    User.XtremeIdiotsId(), id, playerId);

                this.AddAlertSuccess($"The {adminActionData.Type} has been successfully deleted from {playerData.Username}");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting admin action {AdminActionId} for player {PlayerId}", id, playerId);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("AdminActionId", id.ToString());
                errorTelemetry.Properties.TryAdd("PlayerId", playerId.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while deleting the admin action. Please try again.");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }
        }
    }
}