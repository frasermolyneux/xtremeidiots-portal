using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

[Authorize(Policy = AuthPolicies.AccessChangeLog)]
public class ChangeLogController(
 TelemetryClient telemetryClient,
 ILogger<ChangeLogController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{

 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(() =>
 {
 TrackSuccessTelemetry("ChangeLogAccessed", nameof(Index), new Dictionary<string, string>
 {
 { "Controller", nameof(ChangeLogController) },
 { "Resource", "ChangeLog" },
 { "Context", "ApplicationUpdates" }
 });

 return Task.FromResult<IActionResult>(View());
 }, nameof(Index));
 }
}