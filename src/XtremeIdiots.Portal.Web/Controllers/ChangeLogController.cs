using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.Constants;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for displaying the change log of application updates and modifications
    /// </summary>
    [Authorize(Policy = AuthPolicies.AccessChangeLog)]
    public class ChangeLogController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IRepositoryApiClient repositoryApiClient;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<ChangeLogController> logger;

        /// <summary>
        /// Initializes a new instance of the ChangeLogController
        /// </summary>
        /// <param name="authorizationService">Service for handling authorization checks</param>
        /// <param name="repositoryApiClient">Client for accessing repository API services</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public ChangeLogController(
            IAuthorizationService authorizationService,
            IRepositoryApiClient repositoryApiClient,
            TelemetryClient telemetryClient,
            ILogger<ChangeLogController> logger)
        {
            this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the change log index page showing application updates and modifications
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>The change log view displaying recent application changes</returns>
        [HttpGet]
        public IActionResult Index(CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} attempting to view change log",
                    User.XtremeIdiotsId());

                logger.LogInformation("User {UserId} successfully accessed change log",
                    User.XtremeIdiotsId());

                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error accessing change log for user {UserId}",
                    User.XtremeIdiotsId());

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Enrich(User);
                exceptionTelemetry.Properties.TryAdd("Controller", "ChangeLog");
                exceptionTelemetry.Properties.TryAdd("Action", "Index");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }
    }
}