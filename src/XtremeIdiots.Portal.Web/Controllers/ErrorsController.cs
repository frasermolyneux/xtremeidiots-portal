using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for handling error display and diagnostics
    /// </summary>
    public class ErrorsController : Controller
    {
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<ErrorsController> logger;

        /// <summary>
        /// Initializes a new instance of the ErrorsController
        /// </summary>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public ErrorsController(
            TelemetryClient telemetryClient,
            ILogger<ErrorsController> logger)
        {
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays error information based on the HTTP status code and user permissions
        /// </summary>
        /// <param name="id">The HTTP status code of the error</param>
        /// <param name="webHostEnvironment">The web host environment service</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Error details for senior admins or generic error view for other users</returns>
        public IActionResult Display(int id, [FromServices] IWebHostEnvironment webHostEnvironment, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User {UserId} accessing error display for status code {StatusCode}",
                    User.XtremeIdiotsId(), id);

                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

                if (User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                {
                    logger.LogInformation("Senior admin {UserId} viewing detailed error information for status code {StatusCode}",
                        User.XtremeIdiotsId(), id);

                    if (context?.Error != null)
                    {
                        // Log the exception details for senior admin access
                        logger.LogWarning("Detailed error information accessed by senior admin {UserId}: {ErrorMessage}",
                            User.XtremeIdiotsId(), context.Error.Message);

                        return Problem(
                            context.Error.StackTrace,
                            title: context.Error.Message);
                    }
                    else
                    {
                        logger.LogInformation("No exception context available for status code {StatusCode}", id);
                        return View(id);
                    }
                }

                // Regular users get the standard error view without sensitive details
                logger.LogInformation("Standard user {UserId} viewing generic error page for status code {StatusCode}",
                    User.XtremeIdiotsId(), id);

                return View(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while displaying error page for status code {StatusCode}", id);

                var errorTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                errorTelemetry.Enrich(User);
                errorTelemetry.Properties.TryAdd("StatusCode", id.ToString());
                telemetryClient.TrackException(errorTelemetry);

                // Fallback to basic error view to prevent error loops
                return View(id);
            }
        }
    }
}