using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing player analytics data and visualizations 
/// </summary>
/// <remarks>
/// This controller provides analytics endpoints for player data visualization and reporting.
/// It includes methods for displaying analytics views and handling legacy API redirects to
/// the new API controller endpoints for JSON data retrieval.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessPlayers)]
public class PlayerAnalyticsController : BaseController
{
 private readonly IRepositoryApiClient repositoryApiClient;

 /// <summary>
 /// Initializes a new instance of the PlayerAnalyticsController class
 /// </summary>
 /// <param name="repositoryApiClient">The repository API client for data access</param>
 /// <param name="telemetryClient">The Application Insights telemetry client</param>
 /// <param name="logger">The logger instance</param>
 /// <param name="configuration">The application configuration</param>
 /// <exception cref="ArgumentNullException">Thrown when any required parameter is null</exception>
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
 /// Displays the player analytics page with date filtering capability
 /// </summary>
 /// <returns>The analytics view with a 3-month date filter range set in ViewBag</returns>
 /// <remarks>
 /// This method provides the main analytics dashboard for viewing player statistics
 /// and trends. The view includes JavaScript components that will call the API
 /// endpoints to retrieve chart data based on the provided date range.
 /// </remarks>
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
 /// Redirects legacy cumulative daily players API calls to the new API controller endpoint
 /// </summary>
 /// <param name="cutoff">The cutoff date to filter data from</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Permanent redirect to the API controller endpoint</returns>
 /// <remarks>
 /// This method maintains backwards compatibility for legacy frontend code
 /// by redirecting to the proper API controller endpoint that handles JSON data.
 /// </remarks>
 [HttpGet]
 public IActionResult GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
 {
 return RedirectPermanent($"/api/PlayerAnalytics/GetCumulativeDailyPlayersJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
 }

 /// <summary>
 /// Redirects legacy new daily players per game API calls to the new API controller endpoint
 /// </summary>
 /// <param name="cutoff">The cutoff date to filter data from</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Permanent redirect to the API controller endpoint</returns>
 /// <remarks>
 /// This method maintains backwards compatibility for legacy frontend code
 /// by redirecting to the proper API controller endpoint that handles JSON data.
 /// </remarks>
 [HttpGet]
 public IActionResult GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
 {
 return RedirectPermanent($"/api/PlayerAnalytics/GetNewDailyPlayersPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
 }

 /// <summary>
 /// Redirects legacy players drop-off per game API calls to the new API controller endpoint
 /// </summary>
 /// <param name="cutoff">The cutoff date to filter data from</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Permanent redirect to the API controller endpoint</returns>
 /// <remarks>
 /// This method maintains backwards compatibility for legacy frontend code
 /// by redirecting to the proper API controller endpoint that handles JSON data.
 /// </remarks>
 [HttpGet]
 public IActionResult GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
 {
 return RedirectPermanent($"/api/PlayerAnalytics/GetPlayersDropOffPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
 }
}
