using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for displaying the change log of application updates and modifications
/// </summary>
[Authorize(Policy = AuthPolicies.AccessChangeLog)]
public class ChangeLogController(
    TelemetryClient telemetryClient,
    ILogger<ChangeLogController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{

    /// <summary>
    /// Displays the change log index page showing application updates and modifications
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The change log view displaying recent application changes</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            TrackSuccessTelemetry("ChangeLogAccessed", "Index", new Dictionary<string, string>
            {
                { "Controller", "ChangeLog" },
                { "Resource", "ChangeLog" },
                { "Context", "ApplicationUpdates" }
            });

            return Task.FromResult<IActionResult>(View());
        }, nameof(Index));
    }
}