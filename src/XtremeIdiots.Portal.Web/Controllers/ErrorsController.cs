using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for handling error display and diagnostics.
/// Provides different error views based on user permissions - senior admins get detailed
/// diagnostic information while regular users get sanitized error pages to prevent
/// information disclosure vulnerabilities.
/// </summary>
public class ErrorsController(
 TelemetryClient telemetryClient,
 ILogger<ErrorsController> logger,
 IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{

 /// <summary>
 /// Displays error information with security-conscious permission-based detail levels.
 /// Senior admins receive full diagnostic information to aid in troubleshooting,
 /// while regular users receive sanitized error pages to prevent information disclosure.
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

 // Regular users get sanitized error views to prevent information disclosure
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

 // Prevent error loops by returning basic view without additional processing
 return View(id);
 }
 }
}
