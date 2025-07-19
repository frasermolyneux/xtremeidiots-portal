using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for external integrations and public-facing API endpoints
    /// </summary>
    public class ExternalController : BaseController
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        /// <summary>
        /// Initializes a new instance of the ExternalController
        /// </summary>
        /// <param name="repositoryApiClient">Client for accessing repository API services</param>
        /// <param name="TelemetryClient">Client for tracking telemetry events</param>
        /// <param name="Logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public ExternalController(
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient TelemetryClient,
            ILogger<ExternalController> Logger,
            IConfiguration configuration)
            : base(TelemetryClient, Logger, configuration)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("External request for latest admin actions view");

                var adminActionDtos = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                    null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

                if (!adminActionDtos.IsSuccess || adminActionDtos.Result?.Data is null)
                {
                    Logger.LogWarning("Failed to retrieve admin actions for external view - API response unsuccessful or null");
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully retrieved {Count} admin actions for external view",
                    adminActionDtos.Result.Data.Items?.Count() ?? 0);

                TrackSuccessTelemetry("LatestAdminActionsViewed", "LatestAdminActions", new Dictionary<string, string>
                {
                    { "Controller", "External" },
                    { "Resource", "AdminActionsView" },
                    { "Count", (adminActionDtos.Result.Data.Items?.Count() ?? 0).ToString() }
                });

                return View(adminActionDtos);
            }, "LatestAdminActions");
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
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("External API request for latest admin actions JSON data");

                var adminActionsApiResponse = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                    null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

                if (!adminActionsApiResponse.IsSuccess || adminActionsApiResponse.Result?.Data?.Items is null)
                {
                    Logger.LogWarning("Failed to retrieve admin actions for external API - API response unsuccessful or data is null");
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

                Logger.LogInformation("Successfully processed {Count} admin actions for external API response", results.Count);

                TrackSuccessTelemetry("LatestAdminActionsApiCalled", "GetLatestAdminActions", new Dictionary<string, string>
                {
                    { "Controller", "External" },
                    { "Resource", "AdminActionsAPI" },
                    { "Count", results.Count.ToString() }
                });

                return Json(results);
            }, "GetLatestAdminActions");
        }
    }
}
