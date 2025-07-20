
using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Provides health check endpoints for application monitoring
/// </summary>
public class HealthCheckController : Controller
{
    /// <summary>
    /// Legacy health check endpoint that redirects to the API health check
    /// </summary>
    /// <returns>Permanent redirect to the API health check endpoint</returns>
    [HttpGet]
    public IActionResult Status()
    {
        return RedirectPermanent("/api/healthcheck/status");
    }
}