using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing player tags and tag operations
/// </summary>
[Authorize(Policy = AuthPolicies.AccessPlayerTags)]
public class TagsController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IRepositoryApiClient repositoryApiClient;

    /// <summary>
    /// Initializes a new instance of the TagsController
    /// </summary>
    /// <param name="authorizationService">Service for handling authorization checks</param>
    /// <param name="repositoryApiClient">Client for repository API operations</param>
    /// <param name="telemetryClient">Client for application telemetry</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null</exception>
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
    /// Retrieves an authorized tag by ID and validates permissions
    /// </summary>
    /// <param name="id">The unique identifier of the tag</param>
    /// <param name="policy">The authorization policy to check</param>
    /// <param name="action">The action being performed for logging purposes</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Tuple containing potential action result for errors and tag data if successful</returns>
    private async Task<(IActionResult? ActionResult, TagDto? Data)> GetAuthorizedTagAsync(
        Guid id,
        string policy,
        string action,
        CancellationToken cancellationToken = default)
    {
        var tagResponse = await repositoryApiClient.Tags.V1.GetTag(id, cancellationToken);

        if (tagResponse.IsNotFound || tagResponse.Result?.Data is null)
        {
            Logger.LogWarning("Tag {TagId} not found when {Action}", id, action);
            return (NotFound(), null);
        }

        if (!tagResponse.IsSuccess)
        {
            Logger.LogWarning("Failed to retrieve tag {TagId} for {Action}", id, action);
            return (RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 }), null);
        }

        var tagData = tagResponse.Result.Data;
        var authResult = await CheckAuthorizationAsync(
            authorizationService,
            tagData,
            policy,
            action,
            nameof(PlayerTagsController),
            $"TagId:{id},TagName:{tagData.Name}",
            tagData);

        return authResult is not null ? (authResult, null) : (null, tagData);
    }

    /// <summary>
    /// Displays the main tags listing page
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with list of tags or error page on failure</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var tagsResponse = await repositoryApiClient.Tags.V1.GetTags(0, 100, cancellationToken);

            if (!tagsResponse.IsSuccess || tagsResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve tags for user {UserId}", User.XtremeIdiotsId());
                return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
            }

            var model = new TagsViewModel
            {
                Tags = tagsResponse.Result.Data.Items.ToList(),
            };

            return View(model);
        }, nameof(Index));
    }

    /// <summary>
    /// Displays the create tag form
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Create tag view or Unauthorized if user lacks permission</returns>
    [HttpGet]
    [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var canCreateTag = await authorizationService.AuthorizeAsync(User, AuthPolicies.CreatePlayerTag);

            if (!canCreateTag.Succeeded)
            {
                TrackUnauthorizedAccessAttempt(nameof(Create), nameof(PlayerTagsController), "AccessCreateForm");
                return Unauthorized();
            }

            return View(new CreateTagViewModel());
        }, nameof(Create));
    }

    /// <summary>
    /// Processes the create tag form submission
    /// </summary>
    /// <param name="model">The create tag view model containing form data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to index on success, returns view with validation errors on failure</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthPolicies.CreatePlayerTag)]
    public async Task<IActionResult> Create(CreateTagViewModel model, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var modelValidationResult = CheckModelState(model);
            if (modelValidationResult is not null) return modelValidationResult;

            var canCreateTag = await authorizationService.AuthorizeAsync(User, AuthPolicies.CreatePlayerTag);

            if (!canCreateTag.Succeeded)
            {
                TrackUnauthorizedAccessAttempt(nameof(Create), nameof(PlayerTagsController), $"TagName:{model.Name}");
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

            TrackSuccessTelemetry("TagCreated", nameof(Create), new Dictionary<string, string>
            {
                { "TagName", model.Name },
                { "UserDefined", model.UserDefined.ToString() }
            });

            this.AddAlertSuccess($"The tag '{model.Name}' has been successfully created");

            return RedirectToAction(nameof(Index));
        }, nameof(Create));
    }

    /// <summary>
    /// Displays the edit tag form for a specific tag
    /// </summary>
    /// <param name="id">The unique identifier of the tag to edit</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Edit tag view with pre-populated data or error result if tag not found or unauthorized</returns>
    [HttpGet]
    [Authorize(Policy = AuthPolicies.EditPlayerTag)]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var (actionResult, tagData) = await GetAuthorizedTagAsync(id, AuthPolicies.EditPlayerTag, nameof(Edit), cancellationToken);
            if (actionResult is not null) return actionResult;

            var model = new EditTagViewModel
            {
                TagId = tagData!.TagId,
                Name = tagData.Name,
                Description = tagData.Description,
                TagHtml = tagData.TagHtml,
                UserDefined = tagData.UserDefined
            };

            return View(model);
        }, nameof(Edit));
    }

    /// <summary>
    /// Processes the edit tag form submission
    /// </summary>
    /// <param name="model">The edit tag view model containing updated form data</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to index on success, returns view with validation errors on failure</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthPolicies.EditPlayerTag)]
    public async Task<IActionResult> Edit(EditTagViewModel model, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var modelValidationResult = CheckModelState(model);
            if (modelValidationResult is not null) return modelValidationResult;

            var (actionResult, originalTagData) = await GetAuthorizedTagAsync(model.TagId, AuthPolicies.EditPlayerTag, nameof(Edit), cancellationToken);
            if (actionResult is not null)
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

            TrackSuccessTelemetry("TagUpdated", nameof(Edit), new Dictionary<string, string>
            {
                { "TagName", model.Name },
                { "TagId", model.TagId.ToString() },
                { "UserDefined", model.UserDefined.ToString() }
            });

            this.AddAlertSuccess($"The tag '{model.Name}' has been successfully updated");

            return RedirectToAction(nameof(Index));
        }, nameof(Edit));
    }

    /// <summary>
    /// Displays the delete confirmation page for a specific tag
    /// </summary>
    /// <param name="id">The unique identifier of the tag to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Delete confirmation view with tag details or error result if tag not found or unauthorized</returns>
    [HttpGet]
    [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var (actionResult, tagData) = await GetAuthorizedTagAsync(id, AuthPolicies.DeletePlayerTag, nameof(Delete), cancellationToken);
            if (actionResult is not null) return actionResult;

            return View(tagData);
        }, nameof(Delete));
    }

    /// <summary>
    /// Processes the confirmed deletion of a tag
    /// </summary>
    /// <param name="id">The unique identifier of the tag to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Redirects to index with success message or error message on failure</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to delete system tags</exception>
    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthPolicies.DeletePlayerTag)]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var (actionResult, tagData) = await GetAuthorizedTagAsync(id, AuthPolicies.DeletePlayerTag, nameof(Delete), cancellationToken);
            if (actionResult is not null)
            {
                if (actionResult is UnauthorizedResult)
                {
                    this.AddAlertDanger($"You do not have permission to delete this tag");
                    return RedirectToAction(nameof(Index));
                }
                return actionResult;
            }

            if (!tagData!.UserDefined)
            {
                if (!User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                {
                    TrackUnauthorizedAccessAttempt(nameof(Delete), "SystemPlayerTag",
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

            TrackSuccessTelemetry("TagDeleted", nameof(Delete), new Dictionary<string, string>
            {
                { "TagName", tagData.Name },
                { "TagId", id.ToString() },
                { "UserDefined", tagData.UserDefined.ToString() }
            });

            this.AddAlertSuccess($"The tag '{tagData.Name}' has been successfully deleted");

            return RedirectToAction(nameof(Index));
        }, nameof(DeleteConfirmed));
    }
}