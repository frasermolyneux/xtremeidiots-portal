using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

[Authorize(Policy = AuthPolicies.AccessHome)]
public class HomeController(
 IAuthorizationService authorizationService,
 TelemetryClient telemetryClient,
 ILogger<HomeController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
 private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

 [HttpGet]
 public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 await Task.CompletedTask;
 return View();
 }, nameof(Index));
 }
}