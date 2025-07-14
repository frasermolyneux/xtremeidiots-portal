using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for the home page and main application entry point
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessHome)]
    public class HomeController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<HomeController> logger;

        /// <summary>
        /// Initializes a new instance of the HomeController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public HomeController(
            IAuthorizationService authorizationService,
            TelemetryClient telemetryClient,
            ILogger<HomeController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the application home page
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Home page view</returns>
        [HttpGet]
        public IActionResult Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing home page", User.XtremeIdiotsId());

                // Note: Authorization is already handled at controller level via AuthPolicies.AccessHome
                // The HomeAuthHandler automatically succeeds for all authenticated users since this is the home page

                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading home page for user {UserId}", User.XtremeIdiotsId());

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("ActionType", "LoadHomePage");
                telemetryClient.TrackException(errorTelemetry);

                throw;
            }
        }
    }
}