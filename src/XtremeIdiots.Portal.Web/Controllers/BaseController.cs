using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Base controller providing common functionality for telemetry, authorization, and error handling.
/// Centralizes these cross-cutting concerns to ensure consistent behavior across all controllers
/// and reduce code duplication while providing comprehensive auditing capabilities.
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
    /// Tracks unauthorized access attempts to enable security monitoring and audit trails.
    /// This is critical for identifying potential security threats and ensuring compliance
    /// with security policies across the gaming community portal.
    /// </summary>
    /// <param name="action">The action being attempted</param>
    /// <param name="resource">The resource type being accessed</param>
    /// <param name="context">Additional context information</param>
    /// <param name="additionalData">Additional data to enrich the telemetry</param>
    protected void TrackUnauthorizedAccessAttempt(string action, string resource, string? context = null, object? additionalData = null)
    {
        Logger.LogWarning("User {UserId} denied access to {Action} on {Resource}",
            User.XtremeIdiotsId(), action, resource);

        var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
            .Enrich(User);

        if (additionalData != null)
        {
            unauthorizedTelemetry.Properties.TryAdd("AdditionalData", additionalData.ToString() ?? "");
        }

        var controllerName = GetType().Name.Replace("Controller", "");
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
    /// Centralizes error telemetry tracking to ensure consistent error reporting across all controllers.
    /// This enables proactive monitoring of system health and helps identify patterns in application failures
    /// that could impact the gaming community experience.
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="action">The action that was being performed</param>
    /// <param name="additionalProperties">Additional properties to include in telemetry</param>
    protected void TrackErrorTelemetry(Exception exception, string action, Dictionary<string, string>? additionalProperties = null)
    {
        var errorTelemetry = new ExceptionTelemetry(exception)
        {
            SeverityLevel = SeverityLevel.Error
        };

        errorTelemetry.Enrich(User);
        errorTelemetry.Properties.TryAdd("Action", action);

        var controllerName = GetType().Name.Replace("Controller", "");
        errorTelemetry.Properties.TryAdd("Controller", controllerName);

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
    /// Provides a consistent authorization check pattern across all controllers to ensure
    /// game-specific permissions are properly enforced. This is critical for maintaining
    /// security boundaries in a multi-game community environment where users have different
    /// permission levels across different games.
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
            return Unauthorized();
        }

        return null;
    }

    /// <summary>
    /// Standardized success telemetry tracking
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
    /// Standardized model validation check
    /// </summary>
    /// <param name="model">The model to return if validation fails</param>
    /// <param name="additionalSetup">Optional additional setup for the model on validation failure</param>
    /// <returns>ActionResult if validation failed, null if valid</returns>
    protected IActionResult? CheckModelState<T>(T model, Action<T>? additionalSetup = null)
    {
        if (!ModelState.IsValid)
        {
            Logger.LogWarning("Invalid model state in {Controller}", GetType().Name);
            additionalSetup?.Invoke(model);
            return View(model);
        }

        return null;
    }

    /// <summary>
    /// Standardized model validation check with async setup
    /// </summary>
    /// <param name="model">The model to return if validation fails</param>
    /// <param name="additionalSetupAsync">Optional async additional setup for the model on validation failure</param>
    /// <returns>ActionResult if validation failed, null if valid</returns>
    protected async Task<IActionResult?> CheckModelStateAsync<T>(T model, Func<T, Task>? additionalSetupAsync = null)
    {
        if (!ModelState.IsValid)
        {
            Logger.LogWarning("Invalid model state in {Controller}", GetType().Name);
            if (additionalSetupAsync != null)
            {
                await additionalSetupAsync(model);
            }
            return View(model);
        }

        return null;
    }

    /// <summary>
    /// Wraps controller actions with comprehensive error handling, logging, and telemetry.
    /// This pattern ensures consistent behavior across all controller actions while providing
    /// detailed audit trails for troubleshooting and monitoring system health in production.
    /// </summary>
    /// <param name="action">The async action to execute</param>
    /// <param name="actionName">Name of the action for logging</param>
    /// <param name="userId">Optional user ID for context</param>
    /// <returns>The result of the action</returns>
    protected async Task<IActionResult> ExecuteWithErrorHandlingAsync(
        Func<Task<IActionResult>> action,
        string actionName,
        string? userId = null)
    {
        try
        {
            var userIdToLog = userId ?? User.XtremeIdiotsId()?.ToString() ?? "Unknown";
            Logger.LogInformation("User {UserId} executing {ActionName} in {Controller}",
                userIdToLog, actionName, GetType().Name.Replace("Controller", ""));

            var result = await action();

            Logger.LogInformation("User {UserId} successfully completed {ActionName} in {Controller}",
                userIdToLog, actionName, GetType().Name.Replace("Controller", ""));

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
