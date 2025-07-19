using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing player analytics data and visualizations
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessPlayers)]
    public class PlayerAnalyticsController : BaseController
    {
        private readonly IRepositoryApiClient repositoryApiClient;

        public PlayerAnalyticsController(
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<PlayerAnalyticsController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
        }

        /// <summary>
        /// Displays the player analytics page
        /// </summary>
        /// <returns>The analytics view with date filter range</returns>
        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                var cutoff = DateTime.UtcNow.AddMonths(-3);
                ViewBag.DateFilterRange = cutoff;

                return await Task.FromResult(View());
            }, "Analytics");
        }

        /// <summary>
        /// Returns cumulative daily players data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to the new API endpoint</returns>
        [HttpGet]
        public IActionResult GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return RedirectPermanent($"/PlayerAnalytics/GetCumulativeDailyPlayersJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
        }

        /// <summary>
        /// Returns new daily players per game data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to the new API endpoint</returns>
        [HttpGet]
        public IActionResult GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return RedirectPermanent($"/PlayerAnalytics/GetNewDailyPlayersPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
        }

        /// <summary>
        /// Returns players drop-off per game data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to the new API endpoint</returns>
        [HttpGet]
        public IActionResult GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return RedirectPermanent($"/PlayerAnalytics/GetPlayersDropOffPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
        }
    }
}
