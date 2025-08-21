using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Handles the main dashboard and home page functionality
/// </summary>
[Authorize(Policy = AuthPolicies.AccessHome)]
public class HomeController(
    TelemetryClient telemetryClient,
    ILogger<HomeController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    // No additional dependencies required for current actions

    /// <summary>
    /// Displays the main dashboard for authenticated users
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Dashboard view with user-specific content</returns>
    [HttpGet]
    public IActionResult Index(CancellationToken cancellationToken = default)
    {
        return View();
    }
}