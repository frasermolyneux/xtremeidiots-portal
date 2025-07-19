using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing player tag assignments
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayerTagsController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public PlayerTagsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<PlayerTagsController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Displays the form to add a tag to a specific player
        /// </summary>
        /// <param name="id">The player ID to add a tag to</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The add player tag view or error response</returns>
        [HttpGet]
        public async Task<IActionResult> Add(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} accessing add player tag form for player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                var canCreatePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreatePlayerTag);
                if (!canCreatePlayerTag.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to create player tag for player {PlayerId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "PlayerTags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Add");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{id}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (playerResponse.IsNotFound)
                {
                    Logger.LogWarning("Player {PlayerId} not found when adding player tag", id);
                    return NotFound();
                }

                if (!playerResponse.IsSuccess || playerResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Failed to retrieve player {PlayerId} for player tag", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);

                if (!tagsResponse.IsSuccess || tagsResponse.Result?.Data?.Items == null)
                {
                    Logger.LogWarning("Failed to retrieve tags for player tag assignment");
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new AddPlayerTagViewModel
                {
                    PlayerId = id,
                    Player = playerResponse.Result.Data,
                    AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList()
                };

                Logger.LogInformation("Successfully loaded add player tag form for user {UserId} and player {PlayerId} with {TagCount} available tags",
                    User.XtremeIdiotsId(), id, model.AvailableTags.Count);

                var eventTelemetry = new EventTelemetry("PlayerTagAddPageViewed")
                    .Enrich(User)
                    .Enrich(playerResponse.Result.Data);
                eventTelemetry.Properties.TryAdd("AvailableTagCount", model.AvailableTags.Count.ToString());
                TelemetryClient.TrackEvent(eventTelemetry);

                return View(model);
            }, "Add", $"id: {id}");
        }

        /// <summary>
        /// Creates a new player tag assignment based on the submitted form data
        /// </summary>
        /// <param name="model">The add player tag view model containing the tag assignment details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or returns the view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddPlayerTagViewModel model, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} attempting to add tag {TagId} to player {PlayerId}",
                    User.XtremeIdiotsId(), model.TagId, model.PlayerId);

                var canCreatePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreatePlayerTag);
                if (!canCreatePlayerTag.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to add tag {TagId} to player {PlayerId}",
                        User.XtremeIdiotsId(), model.TagId, model.PlayerId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "PlayerTags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Add");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{model.PlayerId},TagId:{model.TagId}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    Logger.LogWarning("Invalid model state for adding tag to player {PlayerId}", model.PlayerId);

                    var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                    if (playerResponse.IsSuccess && playerResponse.Result?.Data != null)
                    {
                        model.Player = playerResponse.Result.Data;
                    }
                    var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);
                    if (tagsResponse.IsSuccess && tagsResponse.Result?.Data?.Items != null)
                    {
                        model.AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList();
                    }
                    return View(model);
                }

                var tagResponse = await repositoryApiClient.Tags.V1.GetTag(model.TagId);
                if (!tagResponse.IsSuccess || tagResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Tag {TagId} not found when adding to player {PlayerId}", model.TagId, model.PlayerId);
                    return RedirectToAction("Display", "Errors", new { id = 404 });
                }

                // Check if this tag is UserDefined - only those can be added by users
                if (!tagResponse.Result.Data.UserDefined)
                {
                    Logger.LogWarning("User {UserId} attempted to assign non-user-defined tag {TagId} to player {PlayerId}",
                        User.XtremeIdiotsId(), model.TagId, model.PlayerId);

                    this.AddAlertDanger("This tag cannot be assigned to players as it is not marked as User Defined.");

                    var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                    if (playerResponse.IsSuccess && playerResponse.Result?.Data != null)
                    {
                        model.Player = playerResponse.Result.Data;
                    }

                    var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100);
                    if (tagsResponse.IsSuccess && tagsResponse.Result?.Data?.Items != null)
                    {
                        model.AvailableTags = tagsResponse.Result.Data.Items.Where(t => t.UserDefined).ToList();
                    }

                    return View(model);
                }

                var userProfileIdString = User.UserProfileId();
                if (string.IsNullOrWhiteSpace(userProfileIdString) || !Guid.TryParse(userProfileIdString, out var userProfileId))
                {
                    Logger.LogWarning("Invalid user profile ID for user {UserId} when adding player tag", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 400 });
                }

                var playerTagDto = new PlayerTagDto
                {
                    PlayerId = model.PlayerId,
                    TagId = model.TagId,
                    UserProfileId = userProfileId,
                    Assigned = DateTime.UtcNow
                };

                var response = await repositoryApiClient.Players.V1.AddPlayerTag(model.PlayerId, playerTagDto);

                if (!response.IsSuccess)
                {
                    Logger.LogWarning("Failed to add tag {TagId} to player {PlayerId} for user {UserId}",
                        model.TagId, model.PlayerId, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully added tag '{TagName}' ({TagId}) to player {PlayerId} by user {UserId}",
                    tagResponse.Result.Data.Name, model.TagId, model.PlayerId, User.XtremeIdiotsId());

                // Get player data for telemetry enrichment
                var playerDataResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
                var playerData = playerDataResponse.IsSuccess ? playerDataResponse.Result?.Data : null;

                var eventTelemetry = new EventTelemetry("PlayerTagAdded")
                    .Enrich(User);
                if (playerData != null)
                    eventTelemetry.Enrich(playerData);
                eventTelemetry.Properties.TryAdd("TagId", model.TagId.ToString());
                eventTelemetry.Properties.TryAdd("TagName", tagResponse.Result.Data.Name);
                TelemetryClient.TrackEvent(eventTelemetry);

                this.AddAlertSuccess($"The tag '{tagResponse.Result.Data.Name}' has been successfully added to the player");

                return RedirectToAction("Details", "Players", new { id = model.PlayerId });
            }, "Add", $"PlayerId: {model.PlayerId}, TagId: {model.TagId}");
        }

        /// <summary>
        /// Displays the confirmation page for removing a player tag
        /// </summary>
        /// <param name="id">The player ID to remove the tag from</param>
        /// <param name="playerTagId">The player tag ID to remove</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The remove player tag confirmation view or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> Remove(Guid id, Guid playerTagId, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} accessing remove player tag confirmation for player {PlayerId} and tag {PlayerTagId}",
                    User.XtremeIdiotsId(), id, playerTagId);

                var canDeletePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.DeletePlayerTag);
                if (!canDeletePlayerTag.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to remove player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "PlayerTags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Remove");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{id},PlayerTagId:{playerTagId}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);
                if (playerResponse.IsNotFound || playerResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Player {PlayerId} not found when removing player tag {PlayerTagId}", id, playerTagId);
                    return NotFound();
                }

                var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(id);
                if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result?.Data?.Items == null)
                {
                    Logger.LogWarning("Failed to retrieve player tags for player {PlayerId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerTag = playerTagsResponse.Result.Data.Items.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
                if (playerTag == null)
                {
                    Logger.LogWarning("Player tag {PlayerTagId} not found for player {PlayerId}", playerTagId, id);
                    return RedirectToAction("Display", "Errors", new { id = 404 });
                }

                // Check if the tag is UserDefined - only those can be removed
                if (playerTag.Tag != null && !playerTag.Tag.UserDefined)
                {
                    Logger.LogWarning("User {UserId} attempted to remove non-user-defined player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    this.AddAlertDanger("This tag cannot be removed as it is not marked as User Defined.");
                    return RedirectToAction("Details", "Players", new { id = id });
                }

                ViewBag.Player = playerResponse.Result.Data;

                Logger.LogInformation("Successfully loaded remove player tag confirmation for user {UserId}, player {PlayerId}, and tag {PlayerTagId}",
                    User.XtremeIdiotsId(), id, playerTagId);

                return View(playerTag);
            }, "Remove", $"id: {id}, playerTagId: {playerTagId}");
        }

        /// <summary>
        /// Confirms the removal of a player tag assignment
        /// </summary>
        /// <param name="id">The player ID to remove the tag from</param>
        /// <param name="playerTagId">The player tag ID to remove</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or appropriate error response</returns>
        [HttpPost]
        [ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(Guid id, Guid playerTagId, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} attempting to remove player tag {PlayerTagId} from player {PlayerId}",
                    User.XtremeIdiotsId(), playerTagId, id);

                var canDeletePlayerTag = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.DeletePlayerTag);
                if (!canDeletePlayerTag.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to remove player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "PlayerTags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Remove");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{id},PlayerTagId:{playerTagId}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);
                if (playerResponse.IsNotFound || playerResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Player {PlayerId} not found when removing player tag {PlayerTagId}", id, playerTagId);
                    return NotFound();
                }

                var playerTagsResponse = await repositoryApiClient.Players.V1.GetPlayerTags(id);
                if (!playerTagsResponse.IsSuccess || playerTagsResponse.Result?.Data?.Items == null)
                {
                    Logger.LogWarning("Failed to retrieve player tags for player {PlayerId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerTag = playerTagsResponse.Result.Data.Items.FirstOrDefault(pt => pt.PlayerTagId == playerTagId);
                if (playerTag == null)
                {
                    Logger.LogWarning("Player tag {PlayerTagId} not found for player {PlayerId}", playerTagId, id);
                    return RedirectToAction("Display", "Errors", new { id = 404 });
                }

                // Check if the tag is UserDefined - only those can be removed
                if (playerTag.Tag != null && !playerTag.Tag.UserDefined)
                {
                    Logger.LogWarning("User {UserId} attempted to remove non-user-defined player tag {PlayerTagId} from player {PlayerId}",
                        User.XtremeIdiotsId(), playerTagId, id);

                    this.AddAlertDanger("This tag cannot be removed as it is not marked as User Defined.");
                    return RedirectToAction("Details", "Players", new { id = id });
                }

                var response = await repositoryApiClient.Players.V1.RemovePlayerTag(id, playerTagId);

                if (!response.IsSuccess)
                {
                    Logger.LogWarning("Failed to remove player tag {PlayerTagId} from player {PlayerId} for user {UserId}",
                        playerTagId, id, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully removed player tag '{TagName}' ({PlayerTagId}) from player {PlayerId} by user {UserId}",
                    playerTag.Tag?.Name ?? "Unknown", playerTagId, id, User.XtremeIdiotsId());

                var eventTelemetry = new EventTelemetry("PlayerTagRemoved")
                    .Enrich(User)
                    .Enrich(playerResponse.Result.Data);
                eventTelemetry.Properties.TryAdd("PlayerTagId", playerTagId.ToString());
                eventTelemetry.Properties.TryAdd("TagName", playerTag.Tag?.Name ?? "Unknown");
                TelemetryClient.TrackEvent(eventTelemetry);

                this.AddAlertSuccess($"The tag '{playerTag.Tag?.Name ?? "Unknown"}' has been successfully removed from the player");

                return RedirectToAction("Details", "Players", new { id = id });
            }, "RemoveConfirmed", $"id: {id}, playerTagId: {playerTagId}");
        }
    }
}
