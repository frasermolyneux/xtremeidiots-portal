
using Microsoft.AspNetCore.Mvc;

namespace XtremeIdiots.Portal.Web.Controllers;

public class HealthCheckController : Controller
{

 [HttpGet]
 public IActionResult Status()
 {
 return RedirectPermanent("/api/healthcheck/status");
 }
}