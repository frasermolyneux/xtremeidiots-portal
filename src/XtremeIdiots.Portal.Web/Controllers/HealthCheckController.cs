
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for application health check endpoints, providing status information about external dependencies
    /// </summary>
    [AllowAnonymous]
    public class HealthCheckController : BaseController
    {
        private readonly List<HealthCheckComponent> healthCheckComponents = new();
        private readonly IInvisionApiClient forumsClient;

        /// <summary>
        /// Initializes a new instance of the HealthCheckController
        /// </summary>
        /// <param name="forumsClient">Client for accessing forums API services</param>
        /// <param name="TelemetryClient">Client for tracking telemetry events</param>
        /// <param name="Logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public HealthCheckController(
            IInvisionApiClient forumsClient,
            TelemetryClient TelemetryClient,
            ILogger<HealthCheckController> Logger,
            IConfiguration configuration)
            : base(TelemetryClient, Logger, configuration)
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
                        if (User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                            return new Tuple<bool, string>(false, ex.Message);

                        return new Tuple<bool, string>(false, "Failed to establish connection to the forums API");
                    }
                }
            });
        }

        /// <summary>
        /// Returns the health status of all system components
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>JSON response containing health status of all components</returns>
        [HttpGet]
        public async Task<IActionResult> Status(CancellationToken cancellationToken = default)
        {
            return await ExecuteWithErrorHandlingAsync(async () =>
            {
                Logger.LogInformation("Health check status requested");

                var result = new HealthCheckResponse();

                foreach (var healthCheckComponent in healthCheckComponents)
                {
                    if (healthCheckComponent.HealthFunc != null)
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
        /// Represents a health check response containing the status of all system components
        /// </summary>
        public class HealthCheckResponse
        {
            /// <summary>
            /// Gets a value indicating whether all components are healthy
            /// </summary>
            public bool IsHealthy {
                get { return Components.All(c => c.IsHealthy); }
            }

            /// <summary>
            /// Gets or sets the list of component statuses
            /// </summary>
            public List<HealthCheckComponentStatus> Components { get; set; } = new();
        }

        /// <summary>
        /// Represents a health check component configuration
        /// </summary>
        public class HealthCheckComponent
        {
            /// <summary>
            /// Gets or sets the name of the component
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this component is critical to overall system health
            /// </summary>
            public bool Critical { get; set; }

            /// <summary>
            /// Gets or sets the function to execute for health checking
            /// </summary>
            public Func<Task<Tuple<bool, string>>>? HealthFunc { get; set; }
        }

        /// <summary>
        /// Represents the status of a health check component
        /// </summary>
        public class HealthCheckComponentStatus
        {
            /// <summary>
            /// Gets or sets the name of the component
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this component is critical to overall system health
            /// </summary>
            public bool Critical { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the component is healthy
            /// </summary>
            public bool IsHealthy { get; set; }

            /// <summary>
            /// Gets or sets additional data about the component status
            /// </summary>
            public string? AdditionalData { get; set; }
        }
    }
}
