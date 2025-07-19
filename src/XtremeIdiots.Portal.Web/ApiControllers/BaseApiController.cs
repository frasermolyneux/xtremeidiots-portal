using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.ApiControllers
{
    /// <summary>
    /// Base controller for all API controllers that provides API-centric functionality
    /// with proper HTTP status codes instead of MVC redirects
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly TelemetryClient TelemetryClient;
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;

        protected BaseApiController(
            TelemetryClient telemetryClient,
            ILogger logger,
            IConfiguration configuration)
        {
            TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Tracks unauthorized access attempts for API endpoints
        /// </summary>
        /// <param name="action">The action being performed</param>
        /// <param name="resourceType">The type of resource</param>
        /// <param name="context">Additional context</param>
        /// <param name="additionalData">Additional data for telemetry</param>
        protected void TrackUnauthorizedAccessAttempt(string action, string resourceType, string? context = null, object? additionalData = null)
        {
            var unauthorizedTelemetry = new EventTelemetry("UnauthorizedAccess")
                .Enrich(User);

            var controllerName = GetType().Name.Replace("Controller", "");
            unauthorizedTelemetry.Properties.TryAdd("Controller", controllerName);
            unauthorizedTelemetry.Properties.TryAdd("Action", action);
            unauthorizedTelemetry.Properties.TryAdd("ResourceType", resourceType);

            if (!string.IsNullOrEmpty(context))
                unauthorizedTelemetry.Properties.TryAdd("Context", context);

            if (additionalData != null)
                unauthorizedTelemetry.Properties.TryAdd("AdditionalData", additionalData.ToString());

            TelemetryClient.TrackEvent(unauthorizedTelemetry);
        }

        /// <summary>
        /// Tracks error telemetry for API endpoints
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="action">The action where the error occurred</param>
        /// <param name="additionalProperties">Additional properties for context</param>
        protected void TrackErrorTelemetry(Exception exception, string action, Dictionary<string, string>? additionalProperties = null)
        {
            var errorTelemetry = new ExceptionTelemetry(exception)
                .Enrich(User);

            var controllerName = GetType().Name.Replace("Controller", "");
            errorTelemetry.Properties.TryAdd("Controller", controllerName);
            errorTelemetry.Properties.TryAdd("Action", action);

            if (additionalProperties != null)
            {
                foreach (var kvp in additionalProperties)
                {
                    errorTelemetry.Properties.TryAdd(kvp.Key, kvp.Value);
                }
            }

            TelemetryClient.TrackException(errorTelemetry);
        }

        /// <summary>
        /// Standardized authorization check with API-appropriate responses
        /// </summary>
        /// <param name="authorizationService">The authorization service</param>
        /// <param name="resource">The resource to authorize against</param>
        /// <param name="policy">The authorization policy</param>
        /// <param name="action">The action being performed</param>
        /// <param name="resourceType">The type of resource</param>
        /// <param name="context">Additional context for unauthorized access tracking</param>
        /// <param name="additionalData">Additional data for telemetry</param>
        /// <returns>ActionResult if unauthorized, null if authorized</returns>
        protected async Task<IActionResult?> CheckAuthorizationAsync(
            IAuthorizationService authorizationService,
            object resource,
            string policy,
            string action,
            string resourceType,
            string? context = null,
            object? additionalData = null)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(User, resource, policy);

            if (!authorizationResult.Succeeded)
            {
                TrackUnauthorizedAccessAttempt(action, resourceType, context, additionalData);

                // Return proper HTTP status codes for APIs
                if (User.Identity?.IsAuthenticated == true)
                {
                    // User is authenticated but not authorized - return 403 Forbidden
                    return Forbid();
                }
                else
                {
                    // User is not authenticated - return 401 Unauthorized
                    return Unauthorized();
                }
            }

            return null;
        }

        /// <summary>
        /// Standardized success telemetry tracking for API operations
        /// </summary>
        /// <param name="eventName">The name of the successful event</param>
        /// <param name="action">The action that was performed</param>
        /// <param name="additionalProperties">Additional properties to include</param>
        protected void TrackSuccessTelemetry(string eventName, string action, Dictionary<string, string>? additionalProperties = null)
        {
            var successTelemetry = new EventTelemetry(eventName)
                .Enrich(User);

            var controllerName = GetType().Name.Replace("Controller", "");
            successTelemetry.Properties.TryAdd("Controller", controllerName);
            successTelemetry.Properties.TryAdd("Action", action);

            if (additionalProperties != null)
            {
                foreach (var kvp in additionalProperties)
                {
                    successTelemetry.Properties.TryAdd(kvp.Key, kvp.Value);
                }
            }

            TelemetryClient.TrackEvent(successTelemetry);
        }

        /// <summary>
        /// Standardized model validation check for API endpoints
        /// </summary>
        /// <returns>BadRequest with validation errors if model is invalid, null if valid</returns>
        protected IActionResult? CheckModelState()
        {
            if (!ModelState.IsValid)
            {
                Logger.LogWarning("Invalid model state in API controller {Controller}", GetType().Name);
                return BadRequest(ModelState);
            }

            return null;
        }

        /// <summary>
        /// Wraps API controller actions with standardized error handling and logging
        /// Returns proper HTTP status codes for API responses
        /// </summary>
        /// <param name="action">The async action to execute</param>
        /// <param name="actionName">Name of the action for logging</param>
        /// <param name="userId">Optional user ID for context</param>
        /// <returns>The result of the action or appropriate error response</returns>
        protected async Task<IActionResult> ExecuteWithErrorHandlingAsync(
            Func<Task<IActionResult>> action,
            string actionName,
            string? userId = null)
        {
            try
            {
                var userIdToLog = userId ?? User.XtremeIdiotsId()?.ToString() ?? "Unknown";
                Logger.LogInformation("User {UserId} executing API {ActionName} in {Controller}",
                    userIdToLog, actionName, GetType().Name.Replace("Controller", ""));

                var result = await action();

                Logger.LogInformation("User {UserId} successfully completed API {ActionName} in {Controller}",
                    userIdToLog, actionName, GetType().Name.Replace("Controller", ""));

                return result;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogWarning(ex, "Unauthorized access in API {ActionName} in {Controller}", actionName, GetType().Name);
                TrackErrorTelemetry(ex, actionName);
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                Logger.LogWarning(ex, "Bad request in API {ActionName} in {Controller}", actionName, GetType().Name);
                TrackErrorTelemetry(ex, actionName);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                Logger.LogWarning(ex, "Resource not found in API {ActionName} in {Controller}", actionName, GetType().Name);
                TrackErrorTelemetry(ex, actionName);
                return NotFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled error in API {ActionName} in {Controller}", actionName, GetType().Name);
                TrackErrorTelemetry(ex, actionName);
                return StatusCode(500, "An internal server error occurred");
            }
        }

        /// <summary>
        /// Gets a configuration value with an optional fallback
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <param name="fallback">The fallback value if the key is not found</param>
        /// <returns>The configuration value or fallback</returns>
        protected string GetConfigurationValue(string key, string fallback = "")
        {
            return Configuration[key] ?? fallback;
        }
    }
}
