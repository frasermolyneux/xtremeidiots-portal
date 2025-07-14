using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for external integrations and public-facing API endpoints
    /// </summary>
    public class ExternalController : Controller
    {
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<ExternalController> logger;

        /// <summary>
        /// Initializes a new instance of the ExternalController
        /// </summary>
        /// <param name="repositoryApiClient">Client for accessing repository API services</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        public ExternalController(
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<ExternalController> logger)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns a view displaying the latest admin actions for external display purposes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>A view containing the latest admin actions</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to retrieve admin actions</exception>
        [HttpGet]
        public async Task<IActionResult> LatestAdminActions(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("External request for latest admin actions view");

                var adminActionDtos = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                    null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

                if (!adminActionDtos.IsSuccess || adminActionDtos.Result?.Data is null)
                {
                    logger.LogWarning("Failed to retrieve admin actions for external view - API response unsuccessful or null");
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                logger.LogInformation("Successfully retrieved {Count} admin actions for external view",
                    adminActionDtos.Result.Data.Items?.Count() ?? 0);

                return View(adminActionDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving latest admin actions for external view");

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Properties.TryAdd("Action", "LatestAdminActions");
                errorTelemetry.Properties.TryAdd("Controller", "External");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Returns JSON data containing the latest admin actions for external API consumption
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON array of formatted admin action data</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to retrieve admin actions</exception>
        [HttpGet]
        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetLatestAdminActions(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("External API request for latest admin actions JSON data");

                var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                    null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

                if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items is null)
                {
                    logger.LogWarning("Failed to retrieve admin actions for external API - API response unsuccessful or data is null");
                    return RedirectToAction("Display", "Errors", new { id = 500 });
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

                logger.LogInformation("Successfully processed {Count} admin actions for external API response", results.Count);

                return Json(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving latest admin actions for external API");

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Properties.TryAdd("Action", "GetLatestAdminActions");
                errorTelemetry.Properties.TryAdd("Controller", "External");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
    }
}