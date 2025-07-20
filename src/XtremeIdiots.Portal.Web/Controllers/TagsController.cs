using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Tags;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Manages player tag operations including creation, editing, deletion and display .
/// Provides CRUD operations for both user-defined and system tags with role-based authorization.
/// </summary>
/// <remarks>
/// This controller handles tag management for the gaming community portal, supporting both user-defined tags
/// and system tags with different permission levels. System tags require senior admin privileges for deletion,
/// while user-defined tags can be managed by users with appropriate permissions. The controller integrates
/// with the Repository API for persistence and includes comprehensive telemetry tracking for audit purposes.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessPlayerTags)]
public class TagsController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the TagsController with required dependencies for tag management operations.
 /// </summary>
 /// <param name="authorizationService">Service for handling authorization checks and policy validation</param>
 /// <param name="repositoryApiClient">Client for interacting with the Repository API for tag data operations</param>
 /// <param name="telemetryClient">Application Insights telemetry client for tracking user actions and system events</param>
 /// <param name="logger">Logger instance for recording application events and debugging information</param>
 /// <param name="configuration">Application configuration for accessing settings and connection strings</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
 /// Helper method to retrieve a tag with comprehensive authorization checking and error handling.
 /// </summary>
 /// <param name="id">The unique identifier of the tag to retrieve</param>
 /// <param name="policy">The authorization policy name to validate against the current user</param>
 /// <param name="action">The specific action being performed for telemetry and logging purposes</param>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// A tuple containing either an action result (for errors/unauthorized access) or the tag data.
 /// If ActionResult is not null, it should be returned immediately. If tag data is not null, the operation succeeded.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to access the specified tag</exception>
 /// <exception cref="NotFoundException">Thrown when the tag with the specified ID does not exist</exception>
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
 /// Displays a comprehensive list of all available player tags with pagination support.
 /// </summary>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// A view containing the tags list on success, or Redirects to an error page if the operation fails.
 /// The view model includes all available tags for display and management.
 /// </returns>
 /// <exception cref="RepositoryException">Thrown when the repository API fails to retrieve tags data</exception>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks permission to view tags</exception>
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
 /// Displays the form for creating a new player tag with proper authorization validation.
 /// </summary>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// create tag view with an empty model on success, or an unauthorized response if the user lacks permission.
 /// The form includes fields for tag name, description, HTML styling and user-defined status.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks CreatePlayerTag permission</exception>
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
 /// Creates a new player tag based on the submitted form data with comprehensive validation and authorization checks.
 /// </summary>
 /// <param name="model">The create tag view model containing the tag details including name, description, HTML styling and user-defined status</param>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// Redirects to the tags list on successful creation with a success message, or view with validation errors and appropriate feedback.
 /// On authorization failure, displays an error message and form for correction.
 /// </returns>
 /// <exception cref="ArgumentNullException">Thrown when the model parameter is null</exception>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks CreatePlayerTag permission</exception>
 /// <exception cref="RepositoryException">Thrown when the repository API fails to create the tag</exception>
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
 /// Displays the form for editing an existing player tag with pre-populated data and authorization validation.
 /// </summary>
 /// <param name="id">The unique identifier of the tag to edit</param>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// edit tag view with populated form data on success, or appropriate error responses for not found/unauthorized scenarios.
 /// The form includes all editable fields pre-filled with current tag values.
 /// </returns>
 /// <exception cref="ArgumentException">Thrown when the ID parameter is empty or invalid</exception>
 /// <exception cref="NotFoundException">Thrown when the tag with the specified ID does not exist</exception>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks EditPlayerTag permission for this specific tag</exception>
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
 /// Updates an existing player tag based on the submitted form data with comprehensive validation and authorization checks.
 /// </summary>
 /// <param name="model">The edit tag view model containing the updated tag details including ID, name, description, HTML styling and user-defined status</param>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// Redirects to the tags list on successful update with a success message, or view with validation errors and appropriate feedback.
 /// On authorization failure, displays an error message and form for correction.
 /// </returns>
 /// <exception cref="ArgumentNullException">Thrown when the model parameter is null</exception>
 /// <exception cref="NotFoundException">Thrown when the tag with the specified ID does not exist</exception>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks EditPlayerTag permission for this specific tag</exception>
 /// <exception cref="RepositoryException">Thrown when the repository API fails to update the tag</exception>
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
 /// Displays the confirmation page for deleting a player tag with comprehensive authorization and data validation.
 /// </summary>
 /// <param name="id">The unique identifier of the tag to delete</param>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// delete confirmation view displaying tag details for user review, or appropriate error responses for not found/unauthorized scenarios.
 /// The confirmation page shows tag information and warns about the permanent nature of deletion.
 /// </returns>
 /// <exception cref="ArgumentException">Thrown when the ID parameter is empty or invalid</exception>
 /// <exception cref="NotFoundException">Thrown when the tag with the specified ID does not exist</exception>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks DeletePlayerTag permission for this specific tag</exception>
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
 /// Permanently deletes a player tag after confirmation with comprehensive authorization checks and business rule validation.
 /// </summary>
 /// <param name="id">The unique identifier of the tag to delete</param>
 /// <param name="cancellationToken">Token to cancel the async operation if needed</param>
 /// <returns>
 /// Redirects to the tags list on successful deletion with appropriate feedback message, or returns error responses for failures.
 /// System tags require senior admin privileges for deletion, while user-defined tags follow standard authorization.
 /// </returns>
 /// <exception cref="ArgumentException">Thrown when the ID parameter is empty or invalid</exception>
 /// <exception cref="NotFoundException">Thrown when the tag with the specified ID does not exist</exception>
 /// <exception cref="UnauthorizedAccessException">Thrown when the user lacks DeletePlayerTag permission or senior admin access for system tags</exception>
 /// <exception cref="RepositoryException">Thrown when the repository API fails to delete the tag</exception>
 /// <remarks>
 /// This method enforces business rules where system tags (UserDefined = false) require senior admin privileges,
 /// while user-defined tags can be deleted by users with standard DeletePlayerTag permission.
 /// </remarks>
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

 // Additional business logic for system tags
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
