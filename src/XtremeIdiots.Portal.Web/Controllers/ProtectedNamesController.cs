using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing protected player names 
/// </summary>
/// <remarks>
/// This controller provides functionality for creating, viewing and deleting protected player names 
/// in the gaming community management system. Protected names allow players to reserve specific 
/// usernames across different game servers, preventing impersonation and maintaining player identity.
/// The controller enforces authorization policies to ensure only authorized administrators can manage 
/// protected names and provides detailed usage reporting for protected name violations.
/// Integrates with the repository API for data persistence and includes comprehensive telemetry 
/// tracking for security monitoring and administrative auditing.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class ProtectedNamesController : BaseController
{
 private readonly IAuthorizationService authorizationService;
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the <see cref="ProtectedNamesController"/> class
 /// </summary>
 /// <param name="authorizationService">Service for handling authorization checks and policy validation for protected name operations</param>
 /// <param name="repositoryApiClient">Client for accessing the repository API for protected name data operations and player information</param>
 /// <param name="telemetryClient">Application Insights telemetry client for tracking protected name operations and security events</param>
 /// <param name="logger">Logger instance for recording controller operation details and security audit information</param>
 /// <param name="configuration">Application configuration for accessing protected name management settings and feature flags</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
 public ProtectedNamesController(
 IAuthorizationService authorizationService,
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<ProtectedNamesController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 }

 /// <summary>
 /// Displays the list of all protected player names with appropriate authorization validation
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// protected names index view with the list of all protected names on success.
 /// Returns Unauthorized result if user lacks ViewProtectedName permission.
 /// Redirects to error page if API call fails or an unexpected error occurs.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissionview protected names</exception>
 /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 new object(),
 AuthPolicies.ViewProtectedName,
 nameof(Index),
 "ProtectedName",
 "ViewAll");

 if (authResult is not null) return authResult;

 var protectedNamesResponse = await repositoryApiClient.Players.V1.GetProtectedNames(0, 1000);

 if (!protectedNamesResponse.IsSuccess || protectedNamesResponse.Result?.Data?.Items is null)
 {
 Logger.LogWarning("Failed to retrieve protected names for user {UserId}", User.XtremeIdiotsId());
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
 }

 var model = new ProtectedNamesViewModel
 {
 ProtectedNames = protectedNamesResponse.Result.Data.Items.ToList()
 };

 TrackSuccessTelemetry("ProtectedNamesViewed", nameof(Index), new Dictionary<string, string>
 {
 { "ProtectedNamesCount", model.ProtectedNames.Count.ToString() }
 });

 return View(model);
 }, nameof(Index));
 }

 /// <summary>
 /// Displays the form to add a protected name for a specific player with authorization validation
 /// </summary>
 /// <param name="id">The unique identifier of the player to add a protected name for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// add protected name view with player information on success.
 /// Returns NotFound result if the specified player does not exist.
 /// Returns Unauthorized result if user lacks CreateProtectedName permission.
 /// Redirects to error page if API call fails or an unexpected error occurs.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate protected names</exception>
 /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
 [HttpGet]
 public async Task<IActionResult> Add(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 new object(),
 AuthPolicies.CreateProtectedName,
 nameof(Add),
 "ProtectedName",
 $"PlayerId:{id}");

 if (authResult is not null) return authResult;

 var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

 if (playerResponse.IsNotFound)
 {
 Logger.LogWarning("Player {PlayerId} not found when adding protected name", id);
 return NotFound();
 }

 if (!playerResponse.IsSuccess || playerResponse.Result?.Data is null)
 {
 Logger.LogWarning("Failed to retrieve player {PlayerId} for protected name", id);
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
 }

 var model = new CreateProtectedNameViewModel(id)
 {
 Player = playerResponse.Result.Data
 };

 return View(model);
 }, nameof(Add), $"id: {id}");
 }

 /// <summary>
 /// Creates a new protected name for a player based on the submitted form data with comprehensive validation
 /// </summary>
 /// <param name="model">The create protected name view model containing the protection details and player information</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// Redirects to player details page on successful creation.
 /// view with validation errors if model state is invalid or protected name already exists.
 /// Returns NotFound result if the specified player does not exist.
 /// Returns Unauthorized result if user lacks CreateProtectedName permission.
 /// Redirects to error page if API call fails or an unexpected error occurs.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissioncreate protected names</exception>
 /// <exception cref="InvalidOperationException">Thrown when user XtremeIdiotsId is null during protected name creation</exception>
 /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
 [HttpPost]
 [ValidateAntiForgeryToken]
 public async Task<IActionResult> Add(CreateProtectedNameViewModel model, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 // Load player data for authorization and telemetry enrichment
 var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(model.PlayerId, PlayerEntityOptions.None);
 if (playerResponse.IsNotFound)
 {
 Logger.LogWarning("Player {PlayerId} not found when creating protected name", model.PlayerId);
 return NotFound();
 }

 if (!playerResponse.IsSuccess || playerResponse.Result?.Data is null)
 {
 Logger.LogWarning("Player data is null for {PlayerId} when creating protected name", model.PlayerId);
 return BadRequest();
 }

 var playerData = playerResponse.Result.Data;

 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 new object(),
 AuthPolicies.CreateProtectedName,
 nameof(Add),
 "ProtectedName",
 $"PlayerId:{model.PlayerId}",
 playerData);

 if (authResult is not null) return authResult;

 var modelValidationResult = CheckModelState(model, m => m.Player = playerData);
 if (modelValidationResult is not null) return modelValidationResult;

 var createProtectedNameDto = new CreateProtectedNameDto(
 model.PlayerId,
 model.Name,
 User.XtremeIdiotsId() ?? throw new InvalidOperationException("User XtremeIdiotsId is required"));

 var response = await repositoryApiClient.Players.V1.CreateProtectedName(createProtectedNameDto);

 if (!response.IsSuccess)
 {
 if (response.IsConflict)
 {
 Logger.LogWarning("Protected name '{ProtectedName}' already exists for another player when user {UserId} attempted to protect it for player {PlayerId}",
 model.Name, User.XtremeIdiotsId(), model.PlayerId);

 ModelState.AddModelError(nameof(model.Name), "This name is already protected by another player");
 model.Player = playerData;
 return View(model);
 }

 Logger.LogWarning("Failed to create protected name for player {PlayerId} by user {UserId}",
 model.PlayerId, User.XtremeIdiotsId());
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
 }

 TrackSuccessTelemetry("ProtectedNameCreated", nameof(Add), new Dictionary<string, string>
 {
 { "PlayerId", model.PlayerId.ToString() },
 { "ProtectedName", model.Name }
 });

 this.AddAlertSuccess($"Protected name '{model.Name}' has been successfully added");

 return RedirectToAction(nameof(PlayersController.Details), nameof(PlayersController), new { id = model.PlayerId });
 }, nameof(Add), $"PlayerId: {model.PlayerId}");
 }

 /// <summary>
 /// Deletes a protected name by ID with comprehensive authorization and validation
 /// </summary>
 /// <param name="id">The unique identifier of the protected name to delete</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// Redirects to player details page on successful deletion.
 /// Returns NotFound result if the specified protected name does not exist.
 /// Returns Unauthorized result if user lacks DeleteProtectedName permission.
 /// Redirects to error page if API call fails or an unexpected error occurs.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permissiondelete protected names</exception>
 /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
 [HttpGet]
 public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var authResult = await CheckAuthorizationAsync(
 authorizationService,
 new object(),
 AuthPolicies.DeleteProtectedName,
 nameof(Delete),
 "ProtectedName",
 $"ProtectedNameId:{id}");

 if (authResult is not null) return authResult;

 var protectedNameResponse = await repositoryApiClient.Players.V1.GetProtectedName(id);

 if (protectedNameResponse.IsNotFound)
 {
 Logger.LogWarning("Protected name {ProtectedNameId} not found when deleting", id);
 return NotFound();
 }

 if (!protectedNameResponse.IsSuccess || protectedNameResponse.Result?.Data is null)
 {
 Logger.LogWarning("Failed to retrieve protected name {ProtectedNameId} for deletion", id);
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
 }

 var playerId = protectedNameResponse.Result.Data.PlayerId;
 var deleteProtectedNameDto = new DeleteProtectedNameDto(id);
 var response = await repositoryApiClient.Players.V1.DeleteProtectedName(deleteProtectedNameDto);

 if (!response.IsSuccess)
 {
 Logger.LogWarning("Failed to delete protected name {ProtectedNameId} for user {UserId}",
 id, User.XtremeIdiotsId());
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
 }

 TrackSuccessTelemetry("ProtectedNameDeleted", nameof(Delete), new Dictionary<string, string>
 {
 { "ProtectedNameId", id.ToString() },
 { "PlayerId", playerId.ToString() }
 });

 this.AddAlertSuccess("Protected name has been successfully deleted");

 return RedirectToAction(nameof(PlayersController.Details), nameof(PlayersController), new { id = playerId });
 }, nameof(Delete), $"id: {id}");
 }

 /// <summary>
 /// Displays the usage report for a specific protected name showing violation history and statistics
 /// </summary>
 /// <param name="id">The unique identifier of the protected name to generate report for</param>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>
 /// protected name report view with usage statistics and violation history on success.
 /// Returns NotFound result if the specified protected name does not exist.
 /// Redirects to error page if API call fails or an unexpected error occurs.
 /// </returns>
 /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token</exception>
 [HttpGet]
 public async Task<IActionResult> Report(Guid id, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var reportResponse = await repositoryApiClient.Players.V1.GetProtectedNameUsageReport(id);

 if (reportResponse.IsNotFound)
 {
 Logger.LogWarning("Protected name report {ProtectedNameId} not found", id);
 return NotFound();
 }

 if (!reportResponse.IsSuccess || reportResponse.Result?.Data is null)
 {
 Logger.LogWarning("Failed to retrieve protected name report {ProtectedNameId}", id);
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
 }

 var model = new ProtectedNameReportViewModel
 {
 Report = reportResponse.Result.Data
 };

 TrackSuccessTelemetry("ProtectedNameReportViewed", nameof(Report), new Dictionary<string, string>
 {
 { "ProtectedNameId", id.ToString() }
 });

 return View(model);
 }, nameof(Report), $"id: {id}");
 }
}
