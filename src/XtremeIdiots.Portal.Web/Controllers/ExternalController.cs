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
        /// <returns>Redirect to the new API endpoint</returns>
        /// <exception cref="InvalidOperationException">Thrown when unable to retrieve admin actions</exception>
        [HttpGet]
        [EnableCors("CorsPolicy")]
        public IActionResult GetLatestAdminActions(CancellationToken cancellationToken = default)
        {
            return RedirectPermanent("/External/GetLatestAdminActions");
        }
    }
}
