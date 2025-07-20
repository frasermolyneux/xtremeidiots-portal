using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.ApiControllers;

/// <summary>
/// Base controller for API controllers with proper HTTP status codes
/// </summary>
/// <param name="telemetryClient">Application Insights telemetry client for tracking API usage</param>
/// <param name="logger">Logger instance for structured logging of API operations</param>
/// <param name="configuration">Configuration provider for application settings</param>
/// <exception cref="ArgumentNullException">Thrown when any required parameter is null</exception>
[ApiController]
public abstract class BaseApiController(
    TelemetryClient telemetryClient,
    ILogger logger,
    IConfiguration configuration) : ControllerBase
{
    protected readonly TelemetryClient TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    protected readonly IConfiguration Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Standardized model validation check for API endpoints
    /// </summary>
    /// <returns>BadRequest with validation errors if model is invalid, null if valid</returns>
    /// <remarks>
    /// This method provides centralized model validation for API endpoints, returning
    /// structured validation error responses that are compatible with API consumers.
    /// </remarks>
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
    /// <remarks>
    /// Unlike MVC controllers that redirect to login pages, this method returns proper
    /// HTTP status codes: 401 Unauthorized for unauthenticated users and 403 Forbidden
    /// for authenticated users without sufficient permissions. This maintains RESTful
    /// API conventions for external integrations.
    /// </remarks>
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

            return User.Identity?.IsAuthenticated == true
                ? Forbid()        // User is authenticated but not authorized - return 403 Forbidden
                : Unauthorized(); // User is not authenticated - return 401 Unauthorized
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
    /// <remarks>
    /// This method provides standardized error handling for API endpoints with:
    /// - Structured logging for all operations with user context
    /// - Automatic exception to HTTP status code mapping
    /// - Telemetry tracking for all errors and exceptions
    /// - Consistent error response formatting for API consumers
    /// Unlike MVC controllers, returns HTTP status codes instead of redirects.
    /// </remarks>
    protected async Task<IActionResult> ExecuteWithErrorHandlingAsync(
        Func<Task<IActionResult>> action,
        string actionName,
        string? userId = null)
    {
        try
        {
            var userIdToLog = userId ?? User.XtremeIdiotsId() ?? "Unknown";
            var controllerName = GetType().Name.Replace(nameof(Controller), "");

            Logger.LogInformation("User {UserId} executing API {ActionName} in {Controller}",
                userIdToLog, actionName, controllerName);

            var result = await action();

            Logger.LogInformation("User {UserId} successfully completed API {ActionName} in {Controller}",
                userIdToLog, actionName, controllerName);

            return result;
        }
        catch (Exception ex)
        {
            var controllerName = GetType().Name.Replace(nameof(Controller), "");

            return ex switch
            {
                UnauthorizedAccessException unauthorizedException => HandleUnauthorizedException(unauthorizedException, actionName, controllerName),
                ArgumentException argumentException => HandleBadRequestException(argumentException, actionName, controllerName),
                KeyNotFoundException keyNotFoundException => HandleNotFoundException(keyNotFoundException, actionName, controllerName),
                _ => HandleGenericException(ex, actionName, controllerName)
            };
        }
    }

    /// <summary>
    /// Standardized success telemetry tracking for API operations
    /// </summary>
    /// <param name="eventName">The name of the successful event</param>
    /// <param name="action">The action that was performed</param>
    /// <param name="additionalProperties">Additional properties to include</param>
    /// <remarks>
    /// This method creates structured success telemetry events with user context
    /// and controller information for monitoring API performance and usage patterns.
    /// </remarks>
    protected void TrackSuccessTelemetry(string eventName, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var successTelemetry = new EventTelemetry(eventName).Enrich(User);

        var controllerName = GetType().Name.Replace(nameof(Controller), "");
        successTelemetry.Properties.TryAdd(nameof(Controller), controllerName);
        successTelemetry.Properties.TryAdd(nameof(Action), action);

        if (additionalProperties is not null)
        {
            foreach (var kvp in additionalProperties)
            {
                successTelemetry.Properties.TryAdd(kvp.Key, kvp.Value);
            }
        }

        TelemetryClient.TrackEvent(successTelemetry);
    }

    /// <summary>
    /// Tracks unauthorized access attempts for API endpoints
    /// </summary>
    /// <param name="action">The action being performed</param>
    /// <param name="resourceType">The type of resource</param>
    /// <param name="context">Additional context</param>
    /// <param name="additionalData">Additional data for telemetry</param>
    /// <remarks>
    /// This method creates structured telemetry events for unauthorized access attempts,
    /// including user context, controller information and additional metadata for
    /// security monitoring and audit purposes.
    /// </remarks>
    protected void TrackUnauthorizedAccessAttempt(string action, string resourceType, string? context = null, object? additionalData = null)
    {
        var unauthorizedTelemetry = new EventTelemetry("UnauthorizedAccess").Enrich(User);

        var controllerName = GetType().Name.Replace(nameof(Controller), "");
        unauthorizedTelemetry.Properties.TryAdd(nameof(Controller), controllerName);
        unauthorizedTelemetry.Properties.TryAdd(nameof(Action), action);
        unauthorizedTelemetry.Properties.TryAdd("ResourceType", resourceType);

        if (!string.IsNullOrEmpty(context))
            unauthorizedTelemetry.Properties.TryAdd("Context", context);

        if (additionalData is not null)
            unauthorizedTelemetry.Properties.TryAdd("AdditionalData", additionalData.ToString());

        TelemetryClient.TrackEvent(unauthorizedTelemetry);
    }

    /// <summary>
    /// Tracks error telemetry for API endpoints
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="action">The action where the error occurred</param>
    /// <param name="additionalProperties">Additional properties for context</param>
    /// <remarks>
    /// This method creates structured exception telemetry with user context and
    /// controller information to aid in debugging and monitoring API performance.
    /// </remarks>
    protected void TrackErrorTelemetry(Exception exception, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var errorTelemetry = new ExceptionTelemetry(exception).Enrich(User);

        var controllerName = GetType().Name.Replace(nameof(Controller), "");
        errorTelemetry.Properties.TryAdd(nameof(Controller), controllerName);
        errorTelemetry.Properties.TryAdd(nameof(Action), action);

        if (additionalProperties is not null)
        {
            foreach (var kvp in additionalProperties)
            {
                errorTelemetry.Properties.TryAdd(kvp.Key, kvp.Value);
            }
        }

        TelemetryClient.TrackException(errorTelemetry);
    }

    /// <summary>
    /// Gets a configuration value with an optional fallback
    /// </summary>
    /// <param name="key">The configuration key</param>
    /// <param name="fallback">The fallback value if the key is not found</param>
    /// <returns>The configuration value or fallback</returns>
    /// <remarks>
    /// This method provides a safe way to access configuration values with fallback support,
    /// useful for API endpoints that need configurable behavior or external service URLs.
    /// </remarks>
    protected string GetConfigurationValue(string key, string fallback = "")
    {
        return Configuration[key] ?? fallback;
    }

    private IActionResult HandleUnauthorizedException(UnauthorizedAccessException ex, string actionName, string controllerName)
    {
        Logger.LogWarning(ex, "Unauthorized access in API {ActionName} in {Controller}", actionName, controllerName);
        TrackErrorTelemetry(ex, actionName);
        return Unauthorized();
    }

    private IActionResult HandleBadRequestException(ArgumentException ex, string actionName, string controllerName)
    {
        Logger.LogWarning(ex, "Bad request in API {ActionName} in {Controller}", actionName, controllerName);
        TrackErrorTelemetry(ex, actionName);
        return BadRequest(ex.Message);
    }

    private IActionResult HandleNotFoundException(KeyNotFoundException ex, string actionName, string controllerName)
    {
        Logger.LogWarning(ex, "Resource not found in API {ActionName} in {Controller}", actionName, controllerName);
        TrackErrorTelemetry(ex, actionName);
        return NotFound();
    }

    private IActionResult HandleGenericException(Exception ex, string actionName, string controllerName)
    {
        Logger.LogError(ex, "Unhandled error in API {ActionName} in {Controller}", actionName, controllerName);
        TrackErrorTelemetry(ex, actionName);
        return StatusCode(500, "An internal server error occurred");
    }
}