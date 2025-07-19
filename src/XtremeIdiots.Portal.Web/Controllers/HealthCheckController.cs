
using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for legacy health check endpoint redirects
/// </summary>
public class HealthCheckController : Controller
{
    /// <summary>
    /// Redirects legacy health check status requests to the new API endpoint
    /// </summary>
    /// <returns>Redirect to the new API endpoint</returns>
    [HttpGet]
    public IActionResult Status()
    {
        return RedirectPermanent("/api/healthcheck/status");
    }
}
