using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for the home page and main application entry point
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessHome)]
    public class HomeController : BaseController
    {
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the HomeController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for application settings</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
                // Authorization is already handled at controller level via AuthPolicies.AccessHome
                // The HomeAuthHandler automatically succeeds for all authenticated users since this is the home page
                await Task.CompletedTask; // Satisfy async requirement for consistency with other controllers
                return View();
            }, "LoadHomePage");
        }
    }
}