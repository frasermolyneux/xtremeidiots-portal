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
        /// <returns>JSON data for cumulative daily players chart</returns>
        [HttpGet]
        public async Task<IActionResult> GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} requesting cumulative daily players data from {Cutoff}",
                    User.XtremeIdiotsId(), cutoff);

                var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetCumulativeDailyPlayers(cutoff);

                if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Failed to retrieve cumulative daily players data for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully retrieved cumulative daily players data for user {UserId}", User.XtremeIdiotsId());
                return Json(playerAnalyticsResponse.Result.Data);
            }, "GetCumulativeDailyPlayersJson", $"cutoff: {cutoff}");
        }

        /// <summary>
        /// Returns new daily players per game data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data for new daily players per game chart</returns>
        [HttpGet]
        public async Task<IActionResult> GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} requesting new daily players per game data from {Cutoff}",
                    User.XtremeIdiotsId(), cutoff);

                var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetNewDailyPlayersPerGame(cutoff);

                if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Failed to retrieve new daily players per game data for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully retrieved new daily players per game data for user {UserId}", User.XtremeIdiotsId());
                return Json(playerAnalyticsResponse.Result.Data);
            }, "GetNewDailyPlayersPerGameJson", $"cutoff: {cutoff}");
        }

        /// <summary>
        /// Returns players drop-off per game data as JSON for analytics charts
        /// </summary>
        /// <param name="cutoff">The cutoff date to filter data from</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON data for players drop-off per game chart</returns>
        [HttpGet]
        public async Task<IActionResult> GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("User {UserId} requesting players drop-off per game data from {Cutoff}",
                    User.XtremeIdiotsId(), cutoff);

                var playerAnalyticsResponse = await repositoryApiClient.PlayerAnalytics.V1.GetPlayersDropOffPerGameJson(cutoff);

                if (!playerAnalyticsResponse.IsSuccess || playerAnalyticsResponse.Result?.Data == null)
                {
                    Logger.LogWarning("Failed to retrieve players drop-off per game data for user {UserId}", User.XtremeIdiotsId());
                    return RedirectToAction("Display", "Errors", new { id = 500 });
                }

                Logger.LogInformation("Successfully retrieved players drop-off per game data for user {UserId}", User.XtremeIdiotsId());
                return Json(playerAnalyticsResponse.Result.Data);
            }, "GetPlayersDropOffPerGameJson", $"cutoff: {cutoff}");
        }
    }
}
