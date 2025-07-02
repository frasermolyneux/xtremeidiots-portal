
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.RepositoryApi.Abstractions.Constants;

namespace XtremeIdiots.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class HealthCheckController : Controller
    {
        private readonly List<HealthCheckComponent> _healthCheckComponents = new();

        public HealthCheckController(IInvisionApiClient forumsClient)
        {
            _healthCheckComponents.Add(new HealthCheckComponent
            {
                Name = "forums-api",
                Critical = true,
                HealthFunc = async () =>
                {
                    try
                    {
                        var response = await forumsClient.Core.GetCoreHello();
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

        [HttpGet]
        public async Task<IActionResult> Status()
        {
            var result = new HealthCheckResponse();

            foreach (var healthCheckComponent in _healthCheckComponents)
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
                actionResult.StatusCode = 503;

            return actionResult;
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