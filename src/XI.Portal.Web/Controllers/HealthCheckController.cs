using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FM.GeoLocation.Contract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XI.Forums.Interfaces;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Data.Legacy;

namespace XI.Portal.Web.Controllers
{
    [AllowAnonymous]
    public class HealthCheckController : Controller
    {
        private readonly List<HealthCheckComponent> _healthCheckComponents = new();

        public HealthCheckController(
            LegacyPortalContext legacyPortalContext,
            IForumsClient forumsClient,
            IGeoLocationClient geoLocationClient)
        {
            _healthCheckComponents.Add(new HealthCheckComponent
            {
                Name = "legacy-database",
                Critical = true,
                HealthFunc = async () =>
                {
                    try
                    {
                        var canConnect = await legacyPortalContext.Database.CanConnectAsync();

                        var additionalData = "OK";
                        if (!canConnect)
                            additionalData = "Failed to establish a connection to the database";

                        return new Tuple<bool, string>(canConnect, additionalData);
                    }
                    catch (Exception ex)
                    {
                        if (User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                            return new Tuple<bool, string>(false, ex.Message);

                        return new Tuple<bool, string>(false, "Failed to establish a connection to the database");
                    }
                }
            });

            _healthCheckComponents.Add(new HealthCheckComponent
            {
                Name = "forums-api",
                Critical = true,
                HealthFunc = async () =>
                {
                    try
                    {
                        var response = await forumsClient.GetCoreHello();
                        var checkResponse = response.CommunityUrl == "https://www.xtremeidiots.com/";
                        return new Tuple<bool, string>(checkResponse, "OK");
                    }
                    catch (Exception ex)
                    {
                        if (User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                            return new Tuple<bool, string>(false, ex.Message);

                        return new Tuple<bool, string>(false, "Failed to establish connection to the forums API");
                    }
                }
            });

            _healthCheckComponents.Add(new HealthCheckComponent
            {
                Name = "geolocation-api",
                Critical = true,
                HealthFunc = async () =>
                {
                    try
                    {
                        var (isHealthy, additionalData) = await geoLocationClient.HealthCheck();

                        if (isHealthy)
                            return new Tuple<bool, string>(true, "OK");

                        if (User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                            return new Tuple<bool, string>(false, additionalData);

                        return new Tuple<bool, string>(false, "Failed to establish connection to the GeoLocation API");
                    }
                    catch (Exception ex)
                    {
                        if (User.HasClaim(claim => claim.Type == XtremeIdiotsClaimTypes.SeniorAdmin))
                            return new Tuple<bool, string>(false, ex.Message);

                        return new Tuple<bool, string>(false, "Failed to establish connection to the GeoLocation API");
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
                var (isHealthy, additionalData) = await healthCheckComponent.HealthFunc.Invoke();

                result.Components.Add(new HealthCheckComponentStatus
                {
                    Name = healthCheckComponent.Name,
                    Critical = healthCheckComponent.Critical,
                    IsHealthy = isHealthy,
                    AdditionalData = additionalData
                });
            }

            var actionResult = new JsonResult(result);

            if (!result.IsHealthy)
                actionResult.StatusCode = 503;

            return actionResult;
        }

        public class HealthCheckResponse
        {
            public bool IsHealthy
            {
                get { return Components.All(c => c.IsHealthy); }
            }

            public List<HealthCheckComponentStatus> Components { get; set; } = new();
        }


        public class HealthCheckComponent
        {
            public string Name { get; set; }
            public bool Critical { get; set; }
            public Func<Task<Tuple<bool, string>>> HealthFunc { get; set; }
        }

        public class HealthCheckComponentStatus
        {
            public string Name { get; set; }
            public bool Critical { get; set; }
            public bool IsHealthy { get; set; }
            public string AdditionalData { get; set; }
        }
    }
}