using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers
{
    /// <summary>
    /// API controller for external integrations and public-facing API endpoints
    /// </summary>
    [Route("External")]
    public class ExternalController : BaseApiController
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        /// <summary>
        /// Initializes a new instance of the ExternalController
        /// </summary>
        /// <param name="repositoryApiClient">Client for accessing repository API services</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
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
        /// Returns JSON data containing the latest admin actions for external API consumption
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON array of formatted admin action data</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to retrieve admin actions</exception>
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
