using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Base controller providing common functionality for telemetry, authorization and error handling
/// </summary>
public abstract class BaseController(
 TelemetryClient telemetryClient,
 ILogger logger,
 IConfiguration configuration) : Controller
{
    protected readonly TelemetryClient TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    protected readonly IConfiguration Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Tracks unauthorized access attempts for security monitoring
    /// </summary>
    /// <param name="action">The action that was attempted</param>
    /// <param name="resource">The resource that was being accessed</param>
    /// <param name="context">Additional context information</param>
    /// <param name="additionalData">Additional data for telemetry</param>
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

        var controllerName = GetType().Name.Replace("Controller", string.Empty);
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
    /// Tracks error telemetry for consistent error reporting
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="action">The action where the error occurred</param>
    /// <param name="additionalProperties">Additional properties for telemetry context</param>
    protected void TrackErrorTelemetry(Exception exception, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var errorTelemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Error
        };

        errorTelemetry.Enrich(User);
        errorTelemetry.Properties.TryAdd("Action", action);

        var controllerName = GetType().Name.Replace("Controller", string.Empty);
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
    /// Checks authorization with consistent error handling and telemetry
    /// </summary>
    /// <param name="authorizationService">The authorization service</param>
    /// <param name="resource">The resource being accessed</param>
    /// <param name="policy">The authorization policy to evaluate</param>
    /// <param name="action">The action being performed</param>
    /// <param name="resourceType">The type of resource for telemetry</param>
    /// <param name="context">Additional context information</param>
    /// <param name="additionalData">Additional data for unauthorized access tracking</param>
    /// <returns>Unauthorized result if authorization fails, null if authorized</returns>
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
            return Unauthorized();
        }

        return null;
    }

    /// <summary>
    /// Tracks successful operations for telemetry and monitoring
    /// </summary>
    /// <param name="eventName">The name of the telemetry event</param>
    /// <param name="action">The action that was successful</param>
    /// <param name="additionalProperties">Additional properties for telemetry context</param>
    protected void TrackSuccessTelemetry(string eventName, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var successTelemetry = new EventTelemetry(eventName)
        .Enrich(User);

        var controllerName = GetType().Name.Replace("Controller", string.Empty);
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
    /// Validates model state and returns view with errors if invalid
    /// </summary>
    /// <typeparam name="T">The type of the model being validated</typeparam>
    /// <param name="model">The model to validate</param>
    /// <param name="additionalSetup">Setup action to run if model state is invalid</param>
    /// <returns>View with validation errors if invalid, null if valid</returns>
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
    /// Validates model state with async setup and returns view with errors if invalid
    /// </summary>
    /// <typeparam name="T">The type of the model being validated</typeparam>
    /// <param name="model">The model to validate</param>
    /// <param name="additionalSetupAsync">Async setup function to run if model state is invalid</param>
    /// <returns>View with validation errors if invalid, null if valid</returns>
    protected async Task<IActionResult?> CheckModelStateAsync<T>(T model, Func<T, Task>? additionalSetupAsync = null)
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
    /// Wraps controller actions with error handling, logging and telemetry
    /// </summary>
    /// <param name="action">The async action to execute</param>
    /// <param name="actionName">The name of the action for logging</param>
    /// <param name="userId">Specific user ID to use for logging instead of current user</param>
    /// <returns>The result of the action execution</returns>
    /// <exception cref="Exception">Re-throws any exceptions after logging</exception>
    protected async Task<IActionResult> ExecuteWithErrorHandlingAsync(
    Func<Task<IActionResult>> action,
    string actionName,
    string? userId = null)
    {
        try
        {
            var userIdToLog = userId ?? User.XtremeIdiotsId()?.ToString() ?? "Unknown";
            Logger.LogInformation("User {UserId} executing {ActionName} in {Controller}",
            userIdToLog, actionName, GetType().Name.Replace("Controller", string.Empty));

            var result = await action();

            Logger.LogInformation("User {UserId} successfully completed {ActionName} in {Controller}",
            userIdToLog, actionName, GetType().Name.Replace("Controller", string.Empty));

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing {ActionName} in {Controller}", actionName, GetType().Name);
            TrackErrorTelemetry(ex, actionName);
            throw;
        }
    }

    /// <summary>
    /// Gets configuration value with fallback
    /// </summary>
    /// <param name="key">The configuration key to retrieve</param>
    /// <param name="fallback">The fallback value if the key is not found</param>
    /// <returns>The configuration value or fallback if not found</returns>
    protected string GetConfigurationValue(string key, string fallback = "")
    {
        return Configuration[key] ?? fallback;
    }
}
