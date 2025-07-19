using Microsoft.ApplicationInsights;
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
    /// Controller for managing player tags
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayerTags)]
    public class TagsController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

        public TagsController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<TagsController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Helper method to get a tag with authorization checking
        /// </summary>
        /// <param name="id">The tag ID</param>
        /// <param name="policy">The authorization policy to check</param>
        /// <param name="action">The action being performed</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple of action result (if error/unauthorized) and tag data</returns>
        private async Task<(IActionResult? ActionResult, TagDto? Data)> GetAuthorizedTagAsync(
            Guid id,
            string policy,
            string action,
            CancellationToken cancellationToken = default)
        {
            var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id, cancellationToken);

            if (tagResponse.IsNotFound || tagResponse.Result?.Data == null)
            {
                Logger.LogWarning("Tag {TagId} not found when {Action}", id, action);
                return (NotFound(), null);
            }

            if (!tagResponse.IsSuccess)
            {
                Logger.LogWarning("Failed to retrieve tag {TagId} for {Action}", id, action);
                return (RedirectToAction("Display", "Errors", new { id = 500 }), null);
            }

            var tagData = tagResponse.Result.Data;
            var authResult = await CheckAuthorizationAsync(
                authorizationService,
                tagData,
                policy,
                action,
                "PlayerTag",
                $"TagId:{id},TagName:{tagData.Name}",
                tagData);

            return authResult != null ? (authResult, null) : (null, tagData);
        }
        /// <summary>
        /// Displays a list of all available player tags
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>View with tags list or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100, cancellationToken);

                if (!tagsResponse.IsSuccess || tagsResponse.Result?.Data?.Items == null)
                {
                    Logger.LogWarning("Failed to retrieve tags for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new TagsViewModel
                {
                    Tags = tagsResponse.Result.Data.Items.ToList(),
                };

                return View(model);
            }, "Index");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var canCreateTag = await authorizationService.AuthorizeAsync(User, AuthPolicies.CreatePlayerTag);

                if (!canCreateTag.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("Create", "PlayerTag", "AccessCreateForm");
                    return Unauthorized();
                }

                return View(new CreateTagViewModel());
            }, "Create");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var modelValidationResult = CheckModelState(model);
                if (modelValidationResult != null) return modelValidationResult;

                var canCreateTag = await authorizationService.AuthorizeAsync(User, AuthPolicies.CreatePlayerTag);

                if (!canCreateTag.Succeeded)
                {
                    TrackUnauthorizedAccessAttempt("Create", "PlayerTag", $"TagName:{model.Name}");
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
                    Logger.LogWarning("Failed to create tag '{TagName}' for user {UserId}",
                        model.Name, User.XtremeIdiotsId());
                    this.AddAlertDanger($"Failed to create tag '{model.Name}'. Please try again.");
                    return View(model);
                }

                TrackSuccessTelemetry("TagCreated", "Create", new Dictionary<string, string>
                {
                    { "TagName", model.Name },
                    { "UserDefined", model.UserDefined.ToString() }
                });

                this.AddAlertSuccess($"The tag '{model.Name}' has been successfully created");

                return RedirectToAction(nameof(Index));
            }, "Create");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, tagData) = await GetAuthorizedTagAsync(id, AuthPolicies.EditPlayerTag, "Edit", cancellationToken);
                if (actionResult != null) return actionResult;

                var model = new EditTagViewModel
                {
                    TagId = tagData!.TagId,
                    Name = tagData.Name,
                    Description = tagData.Description,
                    TagHtml = tagData.TagHtml,
                    UserDefined = tagData.UserDefined
                };

                return View(model);
            }, "Edit");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var modelValidationResult = CheckModelState(model);
                if (modelValidationResult != null) return modelValidationResult;

                var (actionResult, originalTagData) = await GetAuthorizedTagAsync(model.TagId, AuthPolicies.EditPlayerTag, "Edit", cancellationToken);
                if (actionResult != null)
                {
                    if (actionResult is UnauthorizedResult)
                    {
                        this.AddAlertDanger("You do not have permission to edit this tag");
                        return View(model);
                    }
                    return actionResult;
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
                    Logger.LogWarning("Failed to update tag {TagId} '{TagName}' for user {UserId}",
                        model.TagId, model.Name, User.XtremeIdiotsId());
                    this.AddAlertDanger($"Failed to update tag '{model.Name}'. Please try again.");
                    return View(model);
                }

                TrackSuccessTelemetry("TagUpdated", "Edit", new Dictionary<string, string>
                {
                    { "TagName", model.Name },
                    { "TagId", model.TagId.ToString() },
                    { "UserDefined", model.UserDefined.ToString() }
                });

                this.AddAlertSuccess($"The tag '{model.Name}' has been successfully updated");

                return RedirectToAction(nameof(Index));
            }, "Edit");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, tagData) = await GetAuthorizedTagAsync(id, AuthPolicies.DeletePlayerTag, "Delete", cancellationToken);
                if (actionResult != null) return actionResult;

                return View(tagData);
            }, "Delete");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var (actionResult, tagData) = await GetAuthorizedTagAsync(id, AuthPolicies.DeletePlayerTag, "Delete", cancellationToken);
                if (actionResult != null)
                {
                    if (actionResult is UnauthorizedResult)
                    {
                        this.AddAlertDanger($"You do not have permission to delete this tag");
                        return RedirectToAction(nameof(Index));
                    }
                    return actionResult;
                }

                // Additional business logic for system tags
                if (!tagData!.UserDefined)
                {
                    if (!User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                    {
                        TrackUnauthorizedAccessAttempt("Delete", "SystemPlayerTag",
                            $"TagId:{id},TagName:{tagData.Name},RequiresSeniorAdmin:true");

                        this.AddAlertDanger($"You do not have permission to delete the system tag '{tagData.Name}'");
                        return RedirectToAction(nameof(Index));
                    }
                }

                var response = await repositoryApiClient.Tags.V1.DeleteTag(id, cancellationToken);

                if (!response.IsSuccess)
                {
                    Logger.LogWarning("Failed to delete tag {TagId} '{TagName}' for user {UserId}",
                        id, tagData.Name, User.XtremeIdiotsId());
                    this.AddAlertDanger($"Failed to delete tag '{tagData.Name}'. Please try again.");
                    return RedirectToAction(nameof(Index));
                }

                TrackSuccessTelemetry("TagDeleted", "Delete", new Dictionary<string, string>
                {
                    { "TagName", tagData.Name },
                    { "TagId", id.ToString() },
                    { "UserDefined", tagData.UserDefined.ToString() }
                });

                this.AddAlertSuccess($"The tag '{tagData.Name}' has been successfully deleted");

                return RedirectToAction(nameof(Index));
            }, "DeleteConfirmed");
        }
    }
}
