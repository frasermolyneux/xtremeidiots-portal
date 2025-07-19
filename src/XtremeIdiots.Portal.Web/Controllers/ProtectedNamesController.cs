using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.Models;
using XtremeIdiots.Portal.Web.ViewModels;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing protected player names
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class ProtectedNamesController : BaseController
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;

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
        /// Displays the list of all protected player names
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The protected names view with the list of protected names</returns>
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} accessing protected names list", User.XtremeIdiotsId());

                var canViewProtectedNames = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.ViewProtectedName);
                if (!canViewProtectedNames.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to view protected names",
                        User.XtremeIdiotsId());

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ProtectedNames");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Index");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", "ViewAll");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var protectedNamesResponse = await repositoryApiClient.Players.V1.GetProtectedNames(0, 1000);

                if (!protectedNamesResponse.IsSuccess || protectedNamesResponse.Result?.Data?.Items is null)
                {
                    Logger.LogWarning("Failed to retrieve protected names for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new ProtectedNamesViewModel
                {
                    ProtectedNames = protectedNamesResponse.Result.Data.Items.ToList()
                };

                Logger.LogInformation("Successfully retrieved {Count} protected names for user {UserId}",
                    model.ProtectedNames.Count, User.XtremeIdiotsId());

                return View(model);
            }, "Index");
        }

        /// <summary>
        /// Displays the form to add a protected name for a specific player
        /// </summary>
        /// <param name="id">The player ID to add a protected name for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The add protected name view or error response</returns>
        [HttpGet]
        public async Task<IActionResult> Add(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} accessing add protected name form for player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                var canCreateProtectedName = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreateProtectedName);
                if (!canCreateProtectedName.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to create protected name for player {PlayerId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ProtectedNames");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Add");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{id}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var playerResponse = await repositoryApiClient.Players.V1.GetPlayer(id, PlayerEntityOptions.None);

                if (playerResponse.IsNotFound)
                {
                    Logger.LogWarning("Player {PlayerId} not found when adding protected name", id);
                    return NotFound();
                }

                if (!playerResponse.IsSuccess || playerResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Failed to retrieve player {PlayerId} for protected name", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new CreateProtectedNameViewModel(id)
                {
                    Player = playerResponse.Result.Data
                };

                Logger.LogInformation("Successfully loaded add protected name form for user {UserId} and player {PlayerId}",
                    User.XtremeIdiotsId(), id);

                return View(model);
            }, "Add", $"id: {id}");
        }

        /// <summary>
        /// Creates a new protected name for a player based on the submitted form data
        /// </summary>
        /// <param name="model">The create protected name view model containing the protection details</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or returns the view with validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreateProtectedNameViewModel model, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} attempting to create protected name for player {PlayerId}",
                    User.XtremeIdiotsId(), model.PlayerId);

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

                var canCreateProtectedName = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.CreateProtectedName);
                if (!canCreateProtectedName.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to create protected name for player {PlayerId}",
                        User.XtremeIdiotsId(), model.PlayerId);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User)
                        .Enrich(playerData);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ProtectedNames");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Add");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"PlayerId:{model.PlayerId}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    Logger.LogWarning("Invalid model state for creating protected name for player {PlayerId}", model.PlayerId);
                    model.Player = playerData;
                    return View(model);
                }

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

                        ModelState.AddModelError("Name", "This name is already protected by another player");
                        model.Player = playerData;
                        return View(model);
                    }

                    Logger.LogWarning("Failed to create protected name for player {PlayerId} by user {UserId}",
                        model.PlayerId, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully created protected name '{ProtectedName}' for player {PlayerId} by user {UserId}",
                    model.Name, model.PlayerId, User.XtremeIdiotsId());

                var eventTelemetry = new EventTelemetry("ProtectedNameCreated")
                    .Enrich(User)
                    .Enrich(playerData);
                eventTelemetry.Properties.TryAdd("ProtectedName", model.Name);
                TelemetryClient.TrackEvent(eventTelemetry);

                this.AddAlertSuccess($"Protected name '{model.Name}' has been successfully added");

                return RedirectToAction("Details", "Players", new { id = model.PlayerId });
            }, "Add", $"PlayerId: {model.PlayerId}");
        }

        /// <summary>
        /// Deletes a protected name by ID
        /// </summary>
        /// <param name="id">The protected name ID to delete</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirects to player details page on success, or appropriate error response</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} attempting to delete protected name {ProtectedNameId}",
                    User.XtremeIdiotsId(), id);

                var canDeleteProtectedName = await authorizationService.AuthorizeAsync(User, null, AuthPolicies.DeleteProtectedName);
                if (!canDeleteProtectedName.Succeeded)
                {
                    Logger.LogWarning("User {UserId} denied access to delete protected name {ProtectedNameId}",
                        User.XtremeIdiotsId(), id);

                    var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
                        .Enrich(User);
                    unauthorizedTelemetry.Properties.TryAdd("Controller", "ProtectedNames");
                    unauthorizedTelemetry.Properties.TryAdd("Action", "Delete");
                    unauthorizedTelemetry.Properties.TryAdd("Resource", "ProtectedName");
                    unauthorizedTelemetry.Properties.TryAdd("Context", $"ProtectedNameId:{id}");
                    TelemetryClient.TrackEvent(unauthorizedTelemetry);

                    return Unauthorized();
                }

                var protectedNameResponse = await repositoryApiClient.Players.V1.GetProtectedName(id);

                if (protectedNameResponse.IsNotFound)
                {
                    Logger.LogWarning("Protected name {ProtectedNameId} not found when deleting", id);
                    return NotFound();
                }

                if (!protectedNameResponse.IsSuccess || protectedNameResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Failed to retrieve protected name {ProtectedNameId} for deletion", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var playerId = protectedNameResponse.Result.Data.PlayerId;
                var deleteProtectedNameDto = new DeleteProtectedNameDto(id);
                var response = await repositoryApiClient.Players.V1.DeleteProtectedName(deleteProtectedNameDto);

                if (!response.IsSuccess)
                {
                    Logger.LogWarning("Failed to delete protected name {ProtectedNameId} for user {UserId}",
                        id, User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully deleted protected name {ProtectedNameId} by user {UserId}",
                    id, User.XtremeIdiotsId());

                var eventTelemetry = new EventTelemetry("ProtectedNameDeleted")
                    .Enrich(User);
                eventTelemetry.Properties.TryAdd("ProtectedNameId", id.ToString());
                eventTelemetry.Properties.TryAdd("PlayerId", playerId.ToString());
                TelemetryClient.TrackEvent(eventTelemetry);

                this.AddAlertSuccess("Protected name has been successfully deleted");

                return RedirectToAction("Details", "Players", new { id = playerId });
            }, "Delete", $"id: {id}");
        }

        /// <summary>
        /// Displays the usage report for a specific protected name
        /// </summary>
        /// <param name="id">The protected name ID to generate report for</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The protected name report view or error response</returns>
        [HttpGet]
        public async Task<IActionResult> Report(Guid id, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} accessing protected name report for {ProtectedNameId}",
                    User.XtremeIdiotsId(), id);

                var reportResponse = await repositoryApiClient.Players.V1.GetProtectedNameUsageReport(id);

                if (reportResponse.IsNotFound)
                {
                    Logger.LogWarning("Protected name report {ProtectedNameId} not found", id);
                    return NotFound();
                }

                if (!reportResponse.IsSuccess || reportResponse.Result?.Data is null)
                {
                    Logger.LogWarning("Failed to retrieve protected name report {ProtectedNameId}", id);
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                var model = new ProtectedNameReportViewModel
                {
                    Report = reportResponse.Result.Data
                };

                Logger.LogInformation("Successfully retrieved protected name report {ProtectedNameId} for user {UserId}",
                    id, User.XtremeIdiotsId());

                return View(model);
            }, "Report", $"id: {id}");
        }
    }
}
