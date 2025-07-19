using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Controllers;

namespace XtremeIdiots.Portal.Web.ApiControllers
{
    /// <summary>
    /// Base controller for API endpoints that return JSON data and provide RESTful services
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the BaseApiController
        /// </summary>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <param name="configuration">Configuration service for app settings</param>
        protected BaseApiController(
            TelemetryClient telemetryClient,
            ILogger<BaseApiController> logger,
            IConfiguration configuration)
            : base(telemetryClient, logger, configuration)
        {
        }
    }
}
