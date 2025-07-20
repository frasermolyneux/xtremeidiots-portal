using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1;

namespace XtremeIdiots.Portal.Web.ApiControllers
{

    [AllowAnonymous]
    [Route("api/[controller]")]
    public class HealthCheckController : BaseApiController
    {
        private readonly List<HealthCheckComponent> healthCheckComponents = new();
        private readonly IInvisionApiClient forumsClient;

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
                        if (User.HasClaim(claim => claim.Type == UserProfileClaimType.SeniorAdmin))
                            return new Tuple<bool, string>(false, ex.Message);

                        return new Tuple<bool, string>(false, "Failed to establish connection to the forums API");
                    }
                }
            });
        }

        [HttpGet("status")]
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

        public class HealthCheckResponse
        {

            public bool IsHealthy {
                get { return Components.All(c => c.IsHealthy); }
            }

            public List<HealthCheckComponentStatus> Components { get; set; } = new();
        }

        public class HealthCheckComponent
        {

            public string? Name { get; set; }

            public bool Critical { get; set; }

            public Func<Task<Tuple<bool, string>>>? HealthFunc { get; set; }
        }

        public class HealthCheckComponentStatus
        {

            public string? Name { get; set; }

            public bool Critical { get; set; }

            public bool IsHealthy { get; set; }

            public string? AdditionalData { get; set; }
        }
    }
}