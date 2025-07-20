using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Provides player analytics and statistics views, with API endpoints redirected to API controllers
/// </summary>
/// <remarks>
/// Initializes a new instance of the PlayerAnalyticsController
/// </remarks>
/// <param name="repositoryApiClient">Client for accessing repository data</param>
/// <param name="telemetryClient">Client for tracking telemetry data</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class PlayerAnalyticsController(
    IRepositoryApiClient repositoryApiClient,
    TelemetryClient telemetryClient,
    ILogger<PlayerAnalyticsController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    private readonly IRepositoryApiClient repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));

    /// <summary>
    /// Displays the main player analytics dashboard with charting capabilities
    /// </summary>
    /// <returns>Analytics view with date filter settings</returns>
    [HttpGet]
    public async Task<IActionResult> Analytics()
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var cutoff = DateTime.UtcNow.AddMonths(-3);
            ViewBag.DateFilterRange = cutoff;

            return await Task.FromResult(View());
        }, nameof(Analytics));
    }

    /// <summary>
    /// Redirects to API endpoint for cumulative daily players data
    /// </summary>
    /// <param name="cutoff">Date cutoff for data retrieval</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Permanent redirect to API endpoint</returns>
    [HttpGet]
    public IActionResult GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return RedirectPermanent($"/api/PlayerAnalytics/GetCumulativeDailyPlayersJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
    }

    /// <summary>
    /// Redirects to API endpoint for new daily players per game data
    /// </summary>
    /// <param name="cutoff">Date cutoff for data retrieval</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Permanent redirect to API endpoint</returns>
    [HttpGet]
    public IActionResult GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return RedirectPermanent($"/api/PlayerAnalytics/GetNewDailyPlayersPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
    }

    /// <summary>
    /// Redirects to API endpoint for player drop-off per game data
    /// </summary>
    /// <param name="cutoff">Date cutoff for data retrieval</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Permanent redirect to API endpoint</returns>
    [HttpGet]
    public IActionResult GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
    {
        return RedirectPermanent($"/api/PlayerAnalytics/GetPlayersDropOffPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
    }
}