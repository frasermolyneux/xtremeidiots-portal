using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

public abstract class BaseController(
 TelemetryClient telemetryClient,
 ILogger logger,
 IConfiguration configuration) : Controller
{
    protected readonly TelemetryClient TelemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    protected readonly ILogger Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    protected readonly IConfiguration Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

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

    protected string GetConfigurationValue(string key, string fallback = "")
    {
        return Configuration[key] ?? fallback;
    }
}