using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Base controller providing common functionality for all XtremeIdiots Portal MVC controllers
/// </summary>
/// <remarks>
/// Provides standardized error handling, authorization checking, telemetry tracking, and model validation
/// All controllers in the portal must inherit from this base class for consistent behavior
/// </remarks>
public abstract class BaseController(
 TelemetryClient telemetryClient,
 ILogger logger,
 IConfiguration configuration) : Controller
{
    readonly protected TelemetryClient TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    readonly protected ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    readonly protected IConfiguration Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Tracks unauthorized access attempts with telemetry and logging
    /// </summary>
    /// <param name="action">The action that was attempted</param>
    /// <param name="resource">The resource that access was denied to</param>
    /// <param name="context">Optional additional context information</param>
    /// <param name="additionalData">Optional additional data to include in telemetry</param>
    protected void TrackUnauthorizedAccessAttempt(string action, string resource, string? context = null, object? additionalData = null)
    {
        Logger.LogWarning("User {UserId} denied access to {Action} on {Resource}",
        User.XtremeIdiotsId(), action, resource);

        var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
        .Enrich(User);

        if (additionalData is not null)
        {
            unauthorizedTelemetry.Properties.TryAdd("AdditionalData", additionalData.ToString() ?? string.Empty);
        }

        var controllerName = GetControllerNameWithoutSuffix();
        unauthorizedTelemetry.Properties.TryAdd("Controller", controllerName);
        unauthorizedTelemetry.Properties.TryAdd("Action", action);
        unauthorizedTelemetry.Properties.TryAdd("Resource", resource);

        if (!string.IsNullOrEmpty(context))
        {
            unauthorizedTelemetry.Properties.TryAdd("Context", context);
        }

        TelemetryClient.TrackEvent(unauthorizedTelemetry);
    }

    /// <summary>
    /// Tracks error telemetry with exception details and contextual information
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="action">The action where the error occurred</param>
    /// <param name="additionalProperties">Optional additional properties to include</param>
    protected void TrackErrorTelemetry(Exception exception, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var errorTelemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Error
        };

        errorTelemetry.Enrich(User);
        errorTelemetry.Properties.TryAdd("Action", action);

        var controllerName = GetControllerNameWithoutSuffix();
        errorTelemetry.Properties.TryAdd("Controller", controllerName);

        if (additionalProperties is not null)
        {
            foreach (var (key, value) in additionalProperties)
            {
                errorTelemetry.Properties.TryAdd(key, value);
            }
        }

        TelemetryClient.TrackException(errorTelemetry);
    }

    /// <summary>
    /// Checks authorization for a specific resource and policy, returning Unauthorized result if access is denied
    /// </summary>
    /// <param name="authorizationService">The authorization service to use for checking permissions</param>
    /// <param name="resource">The resource being accessed</param>
    /// <param name="policy">The authorization policy to check</param>
    /// <param name="action">The action being performed</param>
    /// <param name="resourceType">The type of resource for telemetry purposes</param>
    /// <param name="context">Optional additional context information</param>
    /// <param name="additionalData">Optional additional data to include in telemetry</param>
    /// <returns>Unauthorized result if access denied, null if access allowed</returns>
    async protected Task<IActionResult?> CheckAuthorizationAsync(
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
            return Unauthorized();
        }

        return null;
    }

    /// <summary>
    /// Tracks successful operations with telemetry
    /// </summary>
    /// <param name="eventName">The name of the event to track</param>
    /// <param name="action">The action that was successful</param>
    /// <param name="additionalProperties">Optional additional properties to include</param>
    protected void TrackSuccessTelemetry(string eventName, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var successTelemetry = new EventTelemetry(eventName)
        .Enrich(User);

        var controllerName = GetControllerNameWithoutSuffix();
        successTelemetry.Properties.TryAdd("Controller", controllerName);
        successTelemetry.Properties.TryAdd("Action", action);

        if (additionalProperties is not null)
        {
            foreach (var (key, value) in additionalProperties)
            {
                successTelemetry.Properties.TryAdd(key, value);
            }
        }

        TelemetryClient.TrackEvent(successTelemetry);
    }

    /// <summary>
    /// Validates model state and returns appropriate view with model if invalid
    /// </summary>
    /// <typeparam name="T">The type of the model being validated</typeparam>
    /// <param name="model">The model to validate</param>
    /// <param name="additionalSetup">Optional additional setup to perform on the model if invalid</param>
    /// <returns>View result with model if invalid, null if valid</returns>
    protected IActionResult? CheckModelState<T>(T model, Action<T>? additionalSetup = null)
    {
        if (!ModelState.IsValid)
        {
            Logger.LogWarning("Invalid model state in {Controller}", nameof(GetType));
            additionalSetup?.Invoke(model);
            return View(model);
        }

        return null;
    }

    /// <summary>
    /// Validates model state asynchronously and returns appropriate view with model if invalid
    /// </summary>
    /// <typeparam name="T">The type of the model being validated</typeparam>
    /// <param name="model">The model to validate</param>
    /// <param name="additionalSetupAsync">Optional asynchronous additional setup to perform on the model if invalid</param>
    /// <returns>View result with model if invalid, null if valid</returns>
    async protected Task<IActionResult?> CheckModelStateAsync<T>(T model, Func<T, Task>? additionalSetupAsync = null)
    {
        if (!ModelState.IsValid)
        {
            Logger.LogWarning("Invalid model state in {Controller}", nameof(GetType));
            if (additionalSetupAsync is not null)
            {
                await additionalSetupAsync(model);
            }

            return View(model);
        }

        return null;
    }

    /// <summary>
    /// Executes an action with standardized error handling and logging
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="actionName">The name of the action for logging purposes</param>
    /// <param name="userId">Optional specific user ID to use in logging</param>
    /// <returns>The result of the action execution</returns>
    /// <exception cref="Exception">Re-throws any exception that occurs during action execution after logging</exception>
    async protected Task<IActionResult> ExecuteWithErrorHandlingAsync(
    Func<Task<IActionResult>> action,
    string actionName,
    string? userId = null)
    {
        try
        {
            var userIdToLog = userId ?? User.XtremeIdiotsId()?.ToString() ?? "Unknown";
            var controllerName = GetControllerNameWithoutSuffix();
            Logger.LogInformation("User {UserId} executing {ActionName} in {Controller}",
            userIdToLog, actionName, controllerName);

            var result = await action();

            Logger.LogInformation("User {UserId} successfully completed {ActionName} in {Controller}",
            userIdToLog, actionName, controllerName);

            return result;
        }
        catch (Exception ex)
        {
            var controllerName = GetControllerNameWithoutSuffix();
            Logger.LogError(ex, "Error executing {ActionName} in {Controller}", actionName, controllerName);
            TrackErrorTelemetry(ex, actionName);
            throw;
        }
    }

    /// <summary>
    /// Gets a configuration value with an optional fallback
    /// </summary>
    /// <param name="key">The configuration key to retrieve</param>
    /// <param name="fallback">The fallback value if the key is not found</param>
    /// <returns>The configuration value or fallback if not found</returns>
    protected string GetConfigurationValue(string key, string fallback = "")
    {
        return Configuration[key] ?? fallback;
    }

    /// <summary>
    /// Gets the controller name without the "Controller" suffix for consistent telemetry and logging
    /// </summary>
    /// <returns>The controller name without suffix</returns>
    private string GetControllerNameWithoutSuffix()
    {
        return GetType().Name.Replace("Controller", string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}