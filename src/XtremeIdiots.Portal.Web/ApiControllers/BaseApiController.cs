using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.ApiControllers;

/// <summary>
/// Base controller for all API controllers providing common functionality
/// </summary>
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
    /// Validates the model state and returns a BadRequest result if invalid
    /// </summary>
    /// <returns>BadRequest result if model state is invalid, null otherwise</returns>
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
    /// Checks authorization for the current user against a policy
    /// </summary>
    /// <param name="authorizationService">Authorization service instance</param>
    /// <param name="resource">Resource being accessed</param>
    /// <param name="policy">Authorization policy to check</param>
    /// <param name="action">Action being performed</param>
    /// <param name="resourceType">Type of resource being accessed</param>
    /// <param name="context">Additional context for the operation</param>
    /// <param name="additionalData">Additional data for authorization</param>
    /// <returns>Unauthorized or Forbidden result if authorization fails, null otherwise</returns>
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
                ? Forbid()
                : Unauthorized();
        }

        return null;
    }

    /// <summary>
    /// Executes an action with standardized error handling and logging
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="actionName">Name of the action for logging</param>
    /// <param name="userId">Optional user ID for logging</param>
    /// <returns>The result of the action or an appropriate error response</returns>
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
    /// Tracks successful operations to Application Insights
    /// </summary>
    /// <param name="eventName">Name of the event</param>
    /// <param name="action">Action being performed</param>
    /// <param name="additionalProperties">Additional properties to track</param>
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
    /// Tracks unauthorized access attempts to Application Insights
    /// </summary>
    /// <param name="action">Action that was attempted</param>
    /// <param name="resourceType">Type of resource being accessed</param>
    /// <param name="context">Additional context</param>
    /// <param name="additionalData">Additional data about the attempt</param>
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
    /// Tracks errors and exceptions to Application Insights
    /// </summary>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="action">Action being performed when error occurred</param>
    /// <param name="additionalProperties">Additional properties to track</param>
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
    /// <param name="key">Configuration key to retrieve</param>
    /// <param name="fallback">Fallback value if key is not found</param>
    /// <returns>Configuration value or fallback</returns>
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