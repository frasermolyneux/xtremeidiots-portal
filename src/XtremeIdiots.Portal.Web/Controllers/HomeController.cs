using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller responsible for the home page and main application entry point.
/// Provides the primary landing page for authenticated users .
/// </summary>
/// <remarks>
/// This controller serves as the main entry point after user authentication and displays
/// the application dashboard. Access is controlled by the AccessHome authorization policy
/// which ensures only authenticated users can view the home page.
/// </remarks>
[Authorize(Policy = AuthPolicies.AccessHome)]
public class HomeController(
 IAuthorizationService authorizationService,
 TelemetryClient telemetryClient,
 ILogger<HomeController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
 private readonly IAuthorizationService authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

 /// <summary>
 /// Displays the application home page with personalized dashboard content.
 /// </summary>
 /// <param name="cancellationToken">Cancellation token to allow operation cancellation</param>
 /// <returns>
 /// home page view for authenticated users.
 /// The view typically contains navigation elements and dashboard widgets.
 /// </returns>
 /// <exception cref="UnauthorizedAccessException">
 /// Thrown when the user does not have permission to access the home page
 /// </exception>
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