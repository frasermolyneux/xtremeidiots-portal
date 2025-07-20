
using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for legacy health check endpoint redirects.
/// Provides backwards compatibility for older health check endpoints by redirecting to the new API.
/// </summary>
public class HealthCheckController : Controller
{
 /// <summary>
 /// Redirects legacy health check status requests to the new API endpoint.
 /// This maintains backwards compatibility for existing monitoring systems that may be using the old endpoint.
 /// </summary>
 /// <returns>A permanent redirect (301) to the new API health check endpoint at /api/healthcheck/status</returns>
 /// <remarks>
 /// This endpoint is maintained for backwards compatibility only. New integrations should use the API endpoint directly.
 /// The permanent redirect ensures search engines and monitoring tools update their references to the new endpoint.
 /// </remarks>
 [HttpGet]
 public IActionResult Status()
 {
 return RedirectPermanent("/api/healthcheck/status");
 }
}
