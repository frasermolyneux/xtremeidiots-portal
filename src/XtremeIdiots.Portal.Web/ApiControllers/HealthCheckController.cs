using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers;

[AllowAnonymous]
[Route("api/[controller]")]
public class HealthCheckController : BaseApiController
{
    private readonly List<HealthCheckComponent> healthCheckComponents = [];
    private readonly IInvisionApiClient forumsClient;

    /// <summary>
    /// Initializes a new instance of the HealthCheckController
    /// </summary>
    /// <param name="forumsClient">The forums API client for health checking the Invision Community integration</param>
    /// <param name="telemetryClient">Application Insights telemetry client for tracking health check events</param>
    /// <param name="logger">Logger instance for health check operations</param>
    /// <param name="configuration">Application configuration</param>
    /// <exception cref="ArgumentNullException">Thrown when forumsClient is null</exception>
    public HealthCheckController(
        IInvisionApiClient forumsClient,
        TelemetryClient telemetryClient,
        ILogger<HealthCheckController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        this.forumsClient = forumsClient ?? throw new ArgumentNullException(nameof(forumsClient));

        healthCheckComponents.Add(new HealthCheckComponent
        {
            Name = "forums-api",
            Critical = true,
            HealthFunc = async () =>
            {
                try
                {
                    var response = await this.forumsClient.Core.GetCoreHello();
                    var checkResponse = response?.CommunityUrl == "https://www.xtremeidiots.com/";
                    return new Tuple<bool, string>(checkResponse, "OK");
                }
                catch (Exception ex)
                {
                    return User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin)
                        ? new Tuple<bool, string>(false, ex.Message)
                        : new Tuple<bool, string>(false, "Failed to establish connection to the forums API");
                }
            }
        });
    }

    /// <summary>
    /// Gets the current health status of all system components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Health check status for all monitored components</returns>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            Logger.LogInformation("Health check status requested");

            var result = new HealthCheckResponse();

            foreach (var healthCheckComponent in healthCheckComponents)
            {
                if (healthCheckComponent.HealthFunc is not null)
                {
                    var (isHealthy, additionalData) = await healthCheckComponent.HealthFunc.Invoke();

                    result.Components.Add(new HealthCheckComponentStatus
                    {
                        Name = healthCheckComponent.Name,
                        Critical = healthCheckComponent.Critical,
                        IsHealthy = isHealthy,
                        AdditionalData = additionalData
                    });
                }
                else
                {
                    result.Components.Add(new HealthCheckComponentStatus
                    {
                        Name = healthCheckComponent.Name,
                        Critical = healthCheckComponent.Critical,
                        IsHealthy = false,
                        AdditionalData = "Invalid health check function"
                    });
                }
            }

            var actionResult = new JsonResult(result);

            if (!result.IsHealthy)
            {
                Logger.LogWarning("Health check failed - one or more components are unhealthy");
                actionResult.StatusCode = 503;

                TrackSuccessTelemetry("HealthCheckFailed", "Status", new Dictionary<string, string>
                {
                    { "Controller", "HealthCheck" },
                    { "Resource", "SystemHealth" },
                    { "IsHealthy", "false" },
                    { "ComponentCount", result.Components.Count.ToString() }
                });
            }
            else
            {
                Logger.LogInformation("Health check completed successfully - all components are healthy");

                TrackSuccessTelemetry("HealthCheckPassed", "Status", new Dictionary<string, string>
                {
                    { "Controller", "HealthCheck" },
                    { "Resource", "SystemHealth" },
                    { "IsHealthy", "true" },
                    { "ComponentCount", result.Components.Count.ToString() }
                });
            }

            return actionResult;
        }, "Status");
    }

    /// <summary>
    /// Represents the overall health check response containing all component statuses
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets a value indicating whether all components are healthy
        /// </summary>
        public bool IsHealthy => Components.All(c => c.IsHealthy);

        /// <summary>
        /// Gets or sets the list of component health statuses
        /// </summary>
        public List<HealthCheckComponentStatus> Components { get; set; } = [];
    }

    /// <summary>
    /// Represents a health check component configuration
    /// </summary>
    public class HealthCheckComponent
    {
        /// <summary>
        /// Gets or sets the name of the component being checked
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component is critical to system operation
        /// </summary>
        public bool Critical { get; set; }

        /// <summary>
        /// Gets or sets the function that performs the health check for this component
        /// </summary>
        public Func<Task<Tuple<bool, string>>>? HealthFunc { get; set; }
    }

    /// <summary>
    /// Represents the status of a single health check component
    /// </summary>
    public class HealthCheckComponentStatus
    {
        /// <summary>
        /// Gets or sets the name of the component
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this component is critical to system operation
        /// </summary>
        public bool Critical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component is healthy
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Gets or sets additional diagnostic data about the component status
        /// </summary>
        public string? AdditionalData { get; set; }
    }
}