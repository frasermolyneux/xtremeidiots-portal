using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for external integrations and public-facing API endpoints that provide data for external consumption
/// </summary>
public class ExternalController : BaseController
{
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the ExternalController for handling external integrations and public API endpoints
 /// </summary>
 /// <param name="repositoryApiClient">Client for accessing repository API services to retrieve admin actions and other data</param>
 /// <param name="telemetryClient">Client for tracking telemetry events and application insights</param>
 /// <param name="logger">Logger for structured logging throughout controller operations</param>
 /// <param name="configuration">Configuration service for accessing application settings</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
 public ExternalController(
 IRepositoryApiClient repositoryApiClient,
 TelemetryClient telemetryClient,
 ILogger<ExternalController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
 }

 /// <summary>
 /// A view displaying the latest admin actions for external display purposes, typically used for embedding in external websites or forum integrations
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation to support request cancellation</param>
 /// <returns>A view containing the latest admin actions data for external consumption</returns>
 /// <exception cref="InvalidOperationException">Thrown when unable to retrieve admin actions from the repository API</exception>
 [HttpGet]
 public async Task<IActionResult> LatestAdminActions(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 Logger.LogInformation("External request for latest admin actions view");

 var adminActionDtos = await repositoryApiClient.AdminActions.V1.GetAdminActions(
 null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

 if (!adminActionDtos.IsSuccess || adminActionDtos.Result?.Data is null)
 {
 Logger.LogWarning("Failed to retrieve admin actions for external view - API response unsuccessful or null");
 return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController).Replace("Controller", ""), new { id = 500 });
 }

 Logger.LogInformation("Successfully retrieved {Count} admin actions for external view",
 adminActionDtos.Result.Data.Items?.Count() ?? 0);

 TrackSuccessTelemetry("LatestAdminActionsViewed", nameof(LatestAdminActions), new Dictionary<string, string>
 {
 { "Controller", nameof(ExternalController).Replace("Controller", "") },
 { "Resource", "AdminActionsView" },
 { "Count", (adminActionDtos.Result.Data.Items?.Count() ?? 0).ToString() }
 });

 return View(adminActionDtos);
 }, nameof(LatestAdminActions));
 }

 /// <summary>
 /// Returns JSON data containing the latest admin actions for external API consumption (deprecated endpoint - Redirects to new API)
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation (not used in redirect)</param>
 /// <returns>Permanent redirect to the new API endpoint for latest admin actions</returns>
 /// <remarks>This endpoint is deprecated and Redirects to maintain backward compatibility with external integrations</remarks>
 [HttpGet]
 [EnableCors("CorsPolicy")]
 public IActionResult GetLatestAdminActions(CancellationToken cancellationToken = default)
 {
 return RedirectPermanent("/External/GetLatestAdminActions");
 }
}
