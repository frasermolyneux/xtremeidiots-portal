using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;
/// <summary>
/// Controller for the home page and main application entry point
/// </summary>
[Authorize(Policy = AuthPolicies.AccessHome)]
public class HomeController : BaseController
{
    private readonly IAuthorizationService authorizationService;

    public HomeController(
        IAuthorizationService authorizationService,
        TelemetryClient telemetryClient,
        ILogger<HomeController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Displays the application home page
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Home page view</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            await Task.CompletedTask;
            return View();
        }, "LoadHomePage");
    }
}