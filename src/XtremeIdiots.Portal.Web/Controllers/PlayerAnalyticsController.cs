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

 [HttpGet]
 public IActionResult GetCumulativeDailyPlayersJson(DateTime cutoff, CancellationToken cancellationToken = default)
 {
 return RedirectPermanent($"/api/PlayerAnalytics/GetCumulativeDailyPlayersJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
 }

 [HttpGet]
 public IActionResult GetNewDailyPlayersPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
 {
 return RedirectPermanent($"/api/PlayerAnalytics/GetNewDailyPlayersPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
 }

 [HttpGet]
 public IActionResult GetPlayersDropOffPerGameJson(DateTime cutoff, CancellationToken cancellationToken = default)
 {
 return RedirectPermanent($"/api/PlayerAnalytics/GetPlayersDropOffPerGameJson?cutoff={cutoff:yyyy-MM-ddTHH:mm:ss}");
 }
}