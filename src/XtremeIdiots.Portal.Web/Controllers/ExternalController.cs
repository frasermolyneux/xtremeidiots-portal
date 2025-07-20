using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Repository.Api.Client.V1;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Handles external API requests for community integrations
/// </summary>
public class ExternalController : BaseController
{
    private readonly IRepositoryApiClient repositoryApiClient;

    /// <summary>
    /// Initializes a new instance of the ExternalController
    /// </summary>
    /// <param name="repositoryApiClient">Repository API client for data access</param>
    /// <param name="telemetryClient">Application Insights telemetry client</param>
    /// <param name="logger">Logger instance for this controller</param>
    /// <param name="configuration">Application configuration</param>
    public ExternalController(
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<ExternalController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.repositoryApiClient = repositoryApiClient ?? throw new ArgumentNullException(nameof(repositoryApiClient));
    }

    /// <summary>
    /// Displays the latest admin actions for external integrations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>View with the latest admin actions</returns>
    [HttpGet]
    public async Task<IActionResult> LatestAdminActions(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            Logger.LogInformation("External request for latest admin actions view");

            var adminActionDtos = await repositoryApiClient.AdminActions.V1.GetAdminActions(
                null, null, null, null, 0, 15, AdminActionOrder.CreatedDesc, cancellationToken);

            if (!adminActionDtos.IsSuccess || adminActionDtos.Result?.Data is null)
            {
                Logger.LogWarning("Failed to retrieve admin actions for external view - API response unsuccessful or null");
                return RedirectToAction(nameof(ErrorsController.Display), nameof(ErrorsController).Replace("Controller", ""), new { id = 500 });
            }

            Logger.LogInformation("Successfully retrieved {Count} admin actions for external view",
                adminActionDtos.Result.Data.Items?.Count() ?? 0);

            TrackSuccessTelemetry("LatestAdminActionsViewed", nameof(LatestAdminActions), new Dictionary<string, string>
            {
                { "Controller", nameof(ExternalController).Replace("Controller", "") },
                { "Resource", "AdminActionsView" },
                { "Count", (adminActionDtos.Result.Data.Items?.Count() ?? 0).ToString() }
            });

            return View(adminActionDtos);
        }, nameof(LatestAdminActions));
    }

    /// <summary>
    /// Legacy endpoint that redirects to the new admin actions API endpoint
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Permanent redirect to the new endpoint</returns>
    [HttpGet]
    [EnableCors("CorsPolicy")]
    public IActionResult GetLatestAdminActions(CancellationToken cancellationToken = default)
    {
        return RedirectPermanent("/External/GetLatestAdminActions");
    }
}