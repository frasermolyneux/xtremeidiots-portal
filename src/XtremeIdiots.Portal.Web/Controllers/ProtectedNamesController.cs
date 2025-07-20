using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Abstractions.Models.V1.Players;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Web.ViewModels;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Manages protected names for players in the XtremeIdiots Portal
/// </summary>
/// <remarks>
/// Initializes a new instance of the ProtectedNamesController
/// </remarks>
/// <param name="authorizationService">Service for handling authorization checks</param>
/// <param name="repositoryApiClient">Client for repository API operations</param>
/// <param name="telemetryClient">Application Insights telemetry client</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class ProtectedNamesController(
    IAuthorizationService authorizationService,
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<ProtectedNamesController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the list of all protected names
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>View with protected names list</returns>
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

            if (authResult is not null)
                return authResult;

            var protectedNamesResponse = await repositoryApiClient.Players.V1.GetProtectedNames(0, 1000);

            if (!protectedNamesResponse.IsSuccess || protectedNamesResponse.Result?.Data?.Items is null)
            {
                Logger.LogWarning("Failed to retrieve protected names for user {UserId}", User.XtremeIdiotsId());
                return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController), new { id = 500 });
            }

            var model = new ProtectedNamesViewModel
            {
                ProtectedNames = [.. protectedNamesResponse.Result.Data.Items]
            };

            TrackSuccessTelemetry("ProtectedNamesViewed", nameof(Index), new Dictionary<string, string>
            {
                { "ProtectedNamesCount", model.ProtectedNames.Count.ToString() }
            });

            return View(model);
        }, nameof(Index));
    }

    /// <summary>
    /// Displays the form to add a protected name for a specific player
    /// </summary>
    /// <param name="id">The player ID to add protected name for</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>View with form to add protected name</returns>
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

            if (authResult is not null)
                return authResult;

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
    /// Processes the form submission to create a new protected name
    /// </summary>
    /// <param name="model">The create protected name view model</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Redirects to player details on success, returns view with errors on failure</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CreateProtectedNameViewModel model, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
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

            if (authResult is not null)
                return authResult;

            var modelValidationResult = CheckModelState(model, m => m.Player = playerData);
            if (modelValidationResult is not null)
                return modelValidationResult;

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
    /// Deletes a protected name by ID
    /// </summary>
    /// <param name="id">The protected name ID to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Redirects to player details on success</returns>
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

            if (authResult is not null)
                return authResult;

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
    /// Displays a usage report for a protected name
    /// </summary>
    /// <param name="id">The protected name ID to generate report for</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>View with protected name usage report</returns>
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