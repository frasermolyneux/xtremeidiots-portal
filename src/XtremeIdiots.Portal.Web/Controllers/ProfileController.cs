using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Handles user profile management operations
/// </summary>
/// <remarks>
/// Initializes a new instance of the ProfileController
/// </remarks>
/// <param name="telemetryClient">Application Insights telemetry client</param>
/// <param name="logger">Logger instance for this controller</param>
/// <param name="configuration">Application configuration</param>
[Authorize(Policy = AuthPolicies.AccessProfile)]
public class ProfileController(
    TelemetryClient telemetryClient,
    ILogger<ProfileController> logger,
    IConfiguration configuration) : BaseController(telemetryClient, logger, configuration)
{
    // No additional dependencies required for current actions

    /// <summary>
    /// Displays the user profile management page
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The profile management view</returns>
    [HttpGet]
    public async Task<IActionResult> Manage(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(() => Task.FromResult<IActionResult>(View()), nameof(Manage));
    }
}