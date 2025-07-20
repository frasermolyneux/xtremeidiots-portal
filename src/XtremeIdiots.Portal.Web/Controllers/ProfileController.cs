using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Web.Auth.Constants;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Handles user profile management operations
/// </summary>
[Authorize(Policy = AuthPolicies.AccessProfile)]
public class ProfileController : BaseController
{
    private readonly IAuthorizationService authorizationService;
    private readonly IRepositoryApiClient repositoryApiClient;

    /// <summary>
    /// Initializes a new instance of the ProfileController
    /// </summary>
    /// <param name="authorizationService">Service for handling authorization checks</param>
    /// <param name="repositoryApiClient">Client for accessing repository API</param>
    /// <param name="telemetryClient">Application Insights telemetry client</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
    public ProfileController(
        IAuthorizationService authorizationService,
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<ProfileController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    /// <summary>
    /// Displays the user profile management page
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The profile management view</returns>
    [HttpGet]
    public async Task<IActionResult> Manage(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(() =>
        {
            return Task.FromResult<IActionResult>(View());
        }, nameof(Manage));
    }
}