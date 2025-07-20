using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for accessing and managing application change logs
/// </summary>
[Authorize(Policy = AuthPolicies.AccessChangeLog)]
public class ChangeLogController(
    TelemetryClient telemetryClient,
    ILogger<ChangeLogController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    /// <summary>
    /// Displays the change log index page showing application updates and version history
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The change log view</returns>
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