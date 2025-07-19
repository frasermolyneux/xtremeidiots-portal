using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for handling error display and diagnostics
    /// </summary>
    public class ErrorsController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the ErrorsController
        /// </summary>
        /// <param name="TelemetryClient">Client for tracking telemetry events</param>
        /// <param name="Logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public ErrorsController(
            TelemetryClient TelemetryClient,
            ILogger<ErrorsController> Logger,
            IConfiguration configuration)
            : base(TelemetryClient, Logger, configuration)
        {
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
                Logger.LogInformation("User {UserId} accessing error display for status code {StatusCode}",
                    User.XtremeIdiotsId(), id);

                var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

                if (User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                {
                    Logger.LogInformation("Senior admin {UserId} viewing detailed error information for status code {StatusCode}",
                        User.XtremeIdiotsId(), id);

                    if (context?.Error != null)
                    {
                        // Log the exception details for senior admin access
                        Logger.LogWarning("Detailed error information accessed by senior admin {UserId}: {ErrorMessage}",
                            User.XtremeIdiotsId(), context.Error.Message);

                        // Track that a senior admin accessed detailed error information
                        TrackSuccessTelemetry("ErrorDetailsViewed", "Display", new Dictionary<string, string>
                        {
                            { "Controller", "Errors" },
                            { "Resource", "ErrorDetails" },
                            { "Context", "SeniorAdminAccess" },
                            { "StatusCode", id.ToString() },
                            { "HasExceptionContext", "true" }
                        });

                        return Problem(
                            context.Error.StackTrace,
                            title: context.Error.Message);
                    }
                    else
                    {
                        Logger.LogInformation("No exception context available for status code {StatusCode}", id);

                        TrackSuccessTelemetry("ErrorPageViewed", "Display", new Dictionary<string, string>
                        {
                            { "Controller", "Errors" },
                            { "Resource", "ErrorPage" },
                            { "Context", "SeniorAdminAccess" },
                            { "StatusCode", id.ToString() },
                            { "HasExceptionContext", "false" }
                        });

                        return View(id);
                    }
                }

                // Regular users get the standard error view without sensitive details
                Logger.LogInformation("Standard user {UserId} viewing generic error page for status code {StatusCode}",
                    User.XtremeIdiotsId(), id);

                TrackSuccessTelemetry("ErrorPageViewed", "Display", new Dictionary<string, string>
                {
                    { "Controller", "Errors" },
                    { "Resource", "ErrorPage" },
                    { "Context", "StandardUserAccess" },
                    { "StatusCode", id.ToString() }
                });

                return View(id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while displaying error page for status code {StatusCode}", id);

                // Use BaseController's error telemetry tracking
                TrackErrorTelemetry(ex, "Display", new Dictionary<string, string>
                {
                    { "StatusCode", id.ToString() },
                    { "Controller", "Errors" },
                    { "Context", "ErrorControllerFailure" }
                });

                // Fallback to basic error view to prevent error loops
                return View(id);
            }
        }
    }
}
