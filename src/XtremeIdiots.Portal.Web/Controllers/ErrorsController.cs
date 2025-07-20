using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Handles error display pages with different detail levels based on user authorization
/// </summary>
/// <remarks>
/// This controller provides error handling and display functionality. Senior admins receive detailed error information
/// including stack traces, while standard users receive generic error pages for security purposes.
/// </remarks>
public class ErrorsController(
    TelemetryClient telemetryClient,
    ILogger<ErrorsController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    /// <summary>
    /// Displays an error page for the given HTTP status code with appropriate detail level
    /// </summary>
    /// <param name="id">The HTTP status code to display an error page for</param>
    /// <param name="webHostEnvironment">The web host environment service</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Error view with detail level based on user authorization</returns>
    /// <remarks>
    /// Senior admins receive detailed error information including stack traces and exception details.
    /// Standard users receive generic error pages to prevent information disclosure.
    /// </remarks>
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

                if (context?.Error is not null)
                {
                    Logger.LogWarning("Detailed error information accessed by senior admin {UserId}: {ErrorMessage}",
                        User.XtremeIdiotsId(), context.Error.Message);

                    TrackSuccessTelemetry(nameof(Display), nameof(ErrorsController), new Dictionary<string, string>
                    {
                        { nameof(ErrorsController), nameof(ErrorsController) },
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

                    TrackSuccessTelemetry(nameof(Display), nameof(ErrorsController), new Dictionary<string, string>
                    {
                        { nameof(ErrorsController), nameof(ErrorsController) },
                        { "Resource", "ErrorPage" },
                        { "Context", "SeniorAdminAccess" },
                        { "StatusCode", id.ToString() },
                        { "HasExceptionContext", "false" }
                    });

                    return View(id);
                }
            }

            Logger.LogInformation("Standard user {UserId} viewing generic error page for status code {StatusCode}",
                User.XtremeIdiotsId(), id);

            TrackSuccessTelemetry(nameof(Display), nameof(ErrorsController), new Dictionary<string, string>
            {
                { nameof(ErrorsController), nameof(ErrorsController) },
                { "Resource", "ErrorPage" },
                { "Context", "StandardUserAccess" },
                { "StatusCode", id.ToString() }
            });

            return View(id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while displaying error page for status code {StatusCode}", id);

            TrackErrorTelemetry(ex, nameof(Display), new Dictionary<string, string>
            {
                { "StatusCode", id.ToString() },
                { nameof(ErrorsController), nameof(ErrorsController) },
                { "Context", "ErrorControllerFailure" }
            });

            return View(id);
        }
    }
}