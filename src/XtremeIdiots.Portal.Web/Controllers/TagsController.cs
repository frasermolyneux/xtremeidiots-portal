using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// Controller for managing player tags
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayerTags)]
    public class TagsController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<TagsController> logger;

        public TagsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<TagsController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Displays a list of all available player tags
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>View with tags list or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing tags list", User.XtremeIdiotsId());

                var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100, cancellationToken);

                if (!tagsResponse.IsSuccess || tagsResponse.Result?.Data?.Items == null)
                {
                    logger.LogWarning("Failed to retrieve tags for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new TagsViewModel
                {
                    Tags = tagsResponse.Result.Data.Items.ToList(),
                };

                logger.LogInformation("Successfully loaded {TagCount} tags for user {UserId}",
                    model.Tags.Count, User.XtremeIdiotsId());

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving tags for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays the form for creating a new player tag
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Create tag view or unauthorized response</returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing create tag form", User.XtremeIdiotsId());

                var canCreateTag = await authorizationService.AuthorizeAsync(User, AuthPolicies.CreatePlayerTag);

                if (!canCreateTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create player tag", User.XtremeIdiotsId());

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", "AccessCreateForm");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                return View(new CreateTagViewModel());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading create tag form for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
        /// <summary>
        /// Creates a new player tag based on the submitted form data
        /// </summary>
        /// <param name="model">The create tag view model containing the tag details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to tags list on success, or returns the view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
        public async Task<IActionResult> Create(CreateTagViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to create tag '{TagName}'",
                    User.XtremeIdiotsId(), model.Name);

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for create tag by user {UserId}", User.XtremeIdiotsId());
                    return View(model);
                }

                var canCreateTag = await authorizationService.AuthorizeAsync(User, AuthPolicies.CreatePlayerTag);

                if (!canCreateTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to create player tag '{TagName}'",
                        User.XtremeIdiotsId(), model.Name);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"TagName:{model.Name}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    this.AddAlertDanger("You do not have permission to create player tags");
                    return View(model);
                }

                var createTagDto = new TagDto
                {
                    Name = model.Name,
                    Description = model.Description,
                    TagHtml = model.TagHtml,
                    UserDefined = model.UserDefined
                };

                var response = await repositoryApiClient.Tags.V1.CreateTag(createTagDto, cancellationToken);

                if (!response.IsSuccess)
                {
                    logger.LogWarning("Failed to create tag '{TagName}' for user {UserId}",
                        model.Name, User.XtremeIdiotsId());
                    this.AddAlertDanger($"Failed to create tag '{model.Name}'. Please try again.");
                    return View(model);
                }

                var tagCreatedTelemetry = new EventTelemetry("TagCreated")
                    .Enrich(User);
                tagCreatedTelemetry.Properties.TryAdd("TagName", model.Name);
                tagCreatedTelemetry.Properties.TryAdd("UserDefined", model.UserDefined.ToString());
                telemetryClient.TrackEvent(tagCreatedTelemetry);

                logger.LogInformation("User {UserId} successfully created tag '{TagName}'",
                    User.XtremeIdiotsId(), model.Name);

                this.AddAlertSuccess($"The tag '{model.Name}' has been successfully created");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating tag '{TagName}' for user {UserId}",
                    model.Name, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("TagName", model.Name ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger($"An error occurred while creating the tag '{model.Name}'. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Displays the form for editing an existing player tag
        /// </summary>
        /// <param name="id">The ID of the tag to edit</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Edit tag view or appropriate error response</returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicies.EditPlayerTag)]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to edit tag {TagId}",
                    User.XtremeIdiotsId(), id);

                var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id, cancellationToken);

                if (tagResponse.IsNotFound)
                {
                    logger.LogWarning("Tag {TagId} not found when editing", id);
                    return NotFound();
                }

                if (!tagResponse.IsSuccess || tagResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve tag {TagId} for editing", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var tagData = tagResponse.Result.Data;
                var canEditTag = await authorizationService.AuthorizeAsync(User, tagData, AuthPolicies.EditPlayerTag);

                if (!canEditTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit tag {TagId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"TagId:{id},TagName:{tagData.Name}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var model = new EditTagViewModel
                {
                    TagId = tagData.TagId,
                    Name = tagData.Name,
                    Description = tagData.Description,
                    TagHtml = tagData.TagHtml,
                    UserDefined = tagData.UserDefined
                };

                logger.LogInformation("Successfully loaded edit form for tag {TagId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading edit form for tag {TagId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("TagId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Updates an existing player tag based on the submitted form data
        /// </summary>
        /// <param name="model">The edit tag view model containing the updated tag details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to tags list on success, or returns the view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.EditPlayerTag)]
        public async Task<IActionResult> Edit(EditTagViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to edit tag {TagId} '{TagName}'",
                    User.XtremeIdiotsId(), model.TagId, model.Name);

                if (!ModelState.IsValid)
                {
                    logger.LogWarning("Invalid model state for edit tag {TagId} by user {UserId}",
                        model.TagId, User.XtremeIdiotsId());
                    return View(model);
                }

                // Get original tag to check permissions and provide context
                var originalTagResponse = await repositoryApiClient.Tags.V1.GetTag(model.TagId, cancellationToken);

                if (originalTagResponse.IsNotFound)
                {
                    logger.LogWarning("Tag {TagId} not found when editing", model.TagId);
                    return NotFound();
                }

                if (!originalTagResponse.IsSuccess || originalTagResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve original tag {TagId} for editing", model.TagId);
                    this.AddAlertDanger("Failed to retrieve tag information. Please try again.");
                    return View(model);
                }

                var originalTagData = originalTagResponse.Result.Data;
                var canEditTag = await authorizationService.AuthorizeAsync(User, originalTagData, AuthPolicies.EditPlayerTag);

                if (!canEditTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to edit tag {TagId} '{TagName}'",
                        User.XtremeIdiotsId(), model.TagId, model.Name);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Edit");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"TagId:{model.TagId},TagName:{model.Name}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    this.AddAlertDanger("You do not have permission to edit this tag");
                    return View(model);
                }

                var tagDto = new TagDto
                {
                    TagId = model.TagId,
                    Name = model.Name,
                    Description = model.Description,
                    TagHtml = model.TagHtml,
                    UserDefined = model.UserDefined
                };

                var response = await repositoryApiClient.Tags.V1.UpdateTag(tagDto, cancellationToken);

                if (!response.IsSuccess)
                {
                    logger.LogWarning("Failed to update tag {TagId} '{TagName}' for user {UserId}",
                        model.TagId, model.Name, User.XtremeIdiotsId());
                    this.AddAlertDanger($"Failed to update tag '{model.Name}'. Please try again.");
                    return View(model);
                }

                var tagUpdatedTelemetry = new EventTelemetry("TagUpdated")
                    .Enrich(User);
                tagUpdatedTelemetry.Properties.TryAdd("TagName", model.Name);
                tagUpdatedTelemetry.Properties.TryAdd("TagId", model.TagId.ToString());
                tagUpdatedTelemetry.Properties.TryAdd("UserDefined", model.UserDefined.ToString());
                telemetryClient.TrackEvent(tagUpdatedTelemetry);

                logger.LogInformation("User {UserId} successfully updated tag {TagId} '{TagName}'",
                    User.XtremeIdiotsId(), model.TagId, model.Name);

                this.AddAlertSuccess($"The tag '{model.Name}' has been successfully updated");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating tag {TagId} '{TagName}' for user {UserId}",
                    model.TagId, model.Name, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("TagId", model.TagId.ToString());
                errorTelemetry.Properties.TryAdd("TagName", model.Name ?? "Unknown");
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger($"An error occurred while updating the tag '{model.Name}'. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// Displays the confirmation page for deleting a player tag
        /// </summary>
        /// <param name="id">The ID of the tag to delete</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Delete confirmation view or appropriate error response</returns>
        [HttpGet]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to view delete confirmation for tag {TagId}",
                    User.XtremeIdiotsId(), id);

                var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id, cancellationToken);

                if (tagResponse.IsNotFound)
                {
                    logger.LogWarning("Tag {TagId} not found when attempting to delete", id);
                    return NotFound();
                }

                if (!tagResponse.IsSuccess || tagResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve tag {TagId} for deletion", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var tagData = tagResponse.Result.Data;
                var canDeleteTag = await authorizationService.AuthorizeAsync(User, tagData, AuthPolicies.DeletePlayerTag);

                if (!canDeleteTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete tag {TagId} '{TagName}'",
                        User.XtremeIdiotsId(), id, tagData.Name);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"TagId:{id},TagName:{tagData.Name}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                logger.LogInformation("Successfully loaded delete confirmation for tag {TagId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                return View(tagData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading delete confirmation for tag {TagId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("TagId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Deletes a player tag after confirmation
        /// </summary>
        /// <param name="id">The ID of the tag to delete</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to tags list on success with appropriate feedback</returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to delete tag {TagId}",
                    User.XtremeIdiotsId(), id);

                var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id, cancellationToken);

                if (tagResponse.IsNotFound)
                {
                    logger.LogWarning("Tag {TagId} not found when attempting to delete", id);
                    return NotFound();
                }

                if (!tagResponse.IsSuccess || tagResponse.Result?.Data == null)
                {
                    logger.LogWarning("Failed to retrieve tag {TagId} for deletion", id);
                    this.AddAlertDanger("Failed to retrieve tag information. Please try again.");
                    return RedirectToAction(nameof(Index));
                }

                var tagData = tagResponse.Result.Data;

                // Check authorization with granular permission logic
                var canDeleteTag = await authorizationService.AuthorizeAsync(User, tagData, AuthPolicies.DeletePlayerTag);

                if (!canDeleteTag.Succeeded)
                {
                    logger.LogWarning("User {UserId} denied access to delete tag {TagId} '{TagName}'",
                        User.XtremeIdiotsId(), id, tagData.Name);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "PlayerTag");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"TagId:{id},TagName:{tagData.Name},UserDefined:{tagData.UserDefined}");
                    telemetryClient.TrackEvent(unauthorizedTelemetry);

                    this.AddAlertDanger($"You do not have permission to delete the tag '{tagData.Name}'");
                    return RedirectToAction(nameof(Index));
                }

                // Additional business logic for system tags (as per existing logic)
                if (!tagData.UserDefined)
                {
                    // Only senior admins can delete non-UserDefined tags
                    if (!User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                    {
                        logger.LogWarning("User {UserId} denied access to delete system tag {TagId} '{TagName}' - insufficient privileges",
                            User.XtremeIdiotsId(), id, tagData.Name);

                        var systemTagUnauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                            .Enrich(User);
                        systemTagUnauthorizedTelemetry.Properties.TryAdd("Controller", "Tags");
                        systemTagUnauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                        systemTagUnauthorizedTelemetry.Properties.TryAdd("Resource", "SystemPlayerTag");
                        systemTagUnauthorizedTelemetry.Properties.TryAdd("Context", $"TagId:{id},TagName:{tagData.Name},RequiresSeniorAdmin:true");
                        telemetryClient.TrackEvent(systemTagUnauthorizedTelemetry);

                        this.AddAlertDanger($"You do not have permission to delete the system tag '{tagData.Name}'");
                        return RedirectToAction(nameof(Index));
                    }
                }

                var response = await repositoryApiClient.Tags.V1.DeleteTag(id, cancellationToken);

                if (!response.IsSuccess)
                {
                    logger.LogWarning("Failed to delete tag {TagId} '{TagName}' for user {UserId}",
                        id, tagData.Name, User.XtremeIdiotsId());
                    this.AddAlertDanger($"Failed to delete tag '{tagData.Name}'. Please try again.");
                    return RedirectToAction(nameof(Index));
                }

                var tagDeletedTelemetry = new EventTelemetry("TagDeleted")
                    .Enrich(User);
                tagDeletedTelemetry.Properties.TryAdd("TagName", tagData.Name);
                tagDeletedTelemetry.Properties.TryAdd("TagId", id.ToString());
                tagDeletedTelemetry.Properties.TryAdd("UserDefined", tagData.UserDefined.ToString());
                telemetryClient.TrackEvent(tagDeletedTelemetry);

                logger.LogInformation("User {UserId} successfully deleted tag {TagId} '{TagName}'",
                    User.XtremeIdiotsId(), id, tagData.Name);

                this.AddAlertSuccess($"The tag '{tagData.Name}' has been successfully deleted");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting tag {TagId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("TagId", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                this.AddAlertDanger("An error occurred while deleting the tag. Please try again.");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
