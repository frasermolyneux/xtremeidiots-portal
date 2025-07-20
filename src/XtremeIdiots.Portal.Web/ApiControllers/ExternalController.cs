using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.ApiControllers;

/// <summary>
/// API controller for external integrations and public-facing endpoints that provide data access for third-party applications and forum integrations.
/// Serves as a RESTful API bridge enabling external systems to access XtremeIdiots Portal data with proper CORS support and structured JSON responses.
/// </summary>
/// <remarks>
/// This controller provides external API endpoints primarily used by:
/// - Forum integrations for displaying admin actions
/// - Third-party applications requiring read-only access to portal data
/// - External services that need to consume portal data via API
/// All endpoints support CORS for cross-domain access and implement proper error handling with structured logging.
/// </remarks>
[Route("External")]
public class ExternalController : BaseApiController
{
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the ExternalController with the required dependencies for external API operations.
 /// </summary>
 /// <param name="repositoryApiClient">Client for accessing repository API services and data retrieval operations</param>
 /// <param name="telemetryClient">Client for tracking telemetry events and monitoring external API usage</param>
 /// <param name="logger">Logger for structured logging of external API operations and error tracking</param>
 /// <param name="configuration">Configuration service for accessing application settings and external service endpoints</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
 /// <remarks>
 /// The constructor establishes dependencies required for:
 /// - Repository data access for admin actions and player information
 /// - Telemetry tracking for monitoring external API performance
 /// - Structured logging for debugging and operational monitoring
 /// - Configuration access for external service integration settings
 /// </remarks>
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
 /// Returns JSON data containing the latest admin actions for external API consumption with structured format for forum integration.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token for the async operation to allow request cancellation</param>
 /// <returns>
 /// JSON array of formatted admin action data containing game icons, admin information, action details and player links.
 /// Returns HTTP 200 OK with admin action array on success, or HTTP 500 on repository service failure.
 /// </returns>
 /// <exception cref="InvalidOperationException">Thrown when unable to retrieve admin actions from repository service</exception>
 /// <remarks>
 /// This endpoint provides external API access to the latest 15 admin actions with:
 /// - Game-specific icon URLs for visual representation
 /// - Admin profile information including display names and forum IDs
 /// - Formatted action text describing the admin action performed
 /// - Direct links to player detail pages for additional context
 /// - Support for expired ban detection with appropriate action text formatting
 /// 
 /// The endpoint is configured with CORS policy to allow cross-domain access for forum integrations
 /// and implements comprehensive error handling with telemetry tracking for monitoring API usage.
 /// </remarks>
 [HttpGet("GetLatestAdminActions")]
 [EnableCors("CorsPolicy")]
 public async Task<IActionResult> GetLatestAdminActions(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 Logger.LogInformation("External API request for latest admin actions JSON data");

 var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(
 null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

 if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items is null)
 {
 Logger.LogWarning("Failed to retrieve admin actions for external API - API response unsuccessful or data is null");
 return StatusCode(500, "Failed to retrieve admin actions data");
 }

 var results = new List<dynamic>();
 foreach (var adminActionDto in adminActionsApiResponse.Result.Data.Items)
 {
 string actionText;
 if (adminActionDto.Expires <= DateTime.UtcNow &&
 (adminActionDto.Type == AdminActionType.Ban || adminActionDto.Type == AdminActionType.TempBan))
 actionText = $"lifted a {adminActionDto.Type} on";
 else
 actionText = $"added a {adminActionDto.Type} to";

 var adminName = adminActionDto.UserProfile?.DisplayName ?? "Unknown";
 var adminId = adminActionDto.UserProfile?.XtremeIdiotsForumId;

 results.Add(new
 {
 GameIconUrl = $"https://portal.xtremeidiots.com/images/game-icons/{adminActionDto.Player?.GameType.ToString()}.png",
 AdminName = adminName,
 AdminId = adminId,
 ActionType = adminActionDto.Type.ToString(),
 ActionText = actionText,
 PlayerName = adminActionDto.Player?.Username,
 PlayerLink = $"https://portal.xtremeidiots.com/Players/Details/{adminActionDto.PlayerId}"
 });
 }

 Logger.LogInformation("Successfully processed {Count} admin actions for external API response", results.Count);

 TrackSuccessTelemetry(nameof(GetLatestAdminActions), nameof(GetLatestAdminActions), new Dictionary<string, string>
 {
 { nameof(ExternalController), nameof(ExternalController) },
 { "Resource", "AdminActionsAPI" },
 { "Count", results.Count.ToString() }
 });

 return Ok(results);
 }, nameof(GetLatestAdminActions));
 }
}
