---
description: 'XtremeIdiots Portal specific patterns and approaches for MVC Controllers'
applyTo: '**/Controllers/**/*.cs'
---

# XtremeIdiots Portal Controller Development Guidelines

> **IMPORTANT**: All formatting and style decisions must follow the project's `.editorconfig` file as the authoritative source of truth.

## Overview

This document outlines the specific patterns, conventions, and approaches used in the XtremeIdiots Portal for building MVC controllers. The portal is a gaming community management system for Call of Duty servers with complex authorization, telemetry, and integration requirements.

## Architecture Context

The XtremeIdiots Portal follows a **service-oriented architecture** with:
- **Web Portal** (MVC Controllers) - User interface and business logic orchestration
- **Repository API** - Data access and persistence layer
- **Forums Integration** - Invision Community forum integration
- **External APIs** - Game servers, geolocation, and other services

## Inheritance and Base Controller

### BaseController Pattern

All controllers **MUST** inherit from `BaseController` which provides:

```csharp
public class ExampleController : BaseController
{
    public ExampleController(
        IAuthorizationService authorizationService,
        IRepositoryApiClient repositoryApiClient,
        TelemetryClient telemetryClient,
        ILogger<ExampleController> logger,
        IConfiguration configuration)
        : base(telemetryClient, logger, configuration)
    {
        // Constructor logic
    }
}
```

### BaseController Features

- **Error Handling**: `ExecuteWithErrorHandlingAsync` for consistent exception handling
- **Authorization**: `CheckAuthorizationAsync` for standardized policy checking
- **Telemetry**: `TrackSuccessTelemetry`, `TrackErrorTelemetry`, `TrackUnauthorizedAccessAttempt`
- **Model Validation**: `CheckModelState` and `CheckModelStateAsync` helpers
- **Configuration**: `GetConfigurationValue` with fallback support

## Error Handling Pattern

### Required Pattern: ExecuteWithErrorHandlingAsync

**ALL public action methods MUST use `ExecuteWithErrorHandlingAsync`:**

```csharp
[HttpGet]
public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
{
    return await ExecuteWithErrorHandlingAsync(async () =>
    {
        // Action implementation
        return View(data);
    }, "ActionDescription");
}
```

### Benefits of This Pattern

- **Consistent Logging**: Automatic entry/exit logging with user context
- **Telemetry Tracking**: Automatic error telemetry and performance metrics
- **Exception Handling**: Centralized exception handling and user-friendly error responses
- **Cancellation Support**: Proper cancellation token propagation

### Error Handling Anti-Patterns

❌ **DON'T use raw try-catch blocks:**
```csharp
// BAD - Inconsistent error handling
public async Task<IActionResult> BadExample()
{
    try
    {
        // Action logic
        return View();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error");
        throw;
    }
}
```

## Authorization Patterns

### Controller-Level Authorization

Controllers **MUST** use policy-based authorization at the class level:

```csharp
[Authorize(Policy = AuthPolicies.AccessBanFileMonitors)]
public class BanFileMonitorsController : BaseController
```

### Action-Level Authorization

Use `CheckAuthorizationAsync` for granular, resource-specific authorization:

```csharp
var authorizationResource = new Tuple<GameType, Guid>(gameServerData.GameType, gameServerData.GameServerId);
var authResult = await CheckAuthorizationAsync(
    authorizationService,
    authorizationResource,
    AuthPolicies.CreateBanFileMonitor,
    "Create",
    "BanFileMonitor",
    $"GameType:{gameServerData.GameType},GameServerId:{gameServerData.GameServerId}",
    gameServerData);

if (authResult != null) return authResult;
```

### Game-Specific Permissions

The portal uses **game-specific permissions** based on user claims:

```csharp
var requiredClaims = new[] { 
    UserProfileClaimType.SeniorAdmin, 
    UserProfileClaimType.HeadAdmin, 
    UserProfileClaimType.GameAdmin, 
    UserProfileClaimType.BanFileMonitor 
};
var (gameTypes, itemIds) = User.ClaimedGamesAndItems(requiredClaims);
```

### Authorization Resource Patterns

- **Simple Resources**: Single game type `(GameType gameType)`
- **Complex Resources**: Tuples for multiple properties `(GameType, Guid, string)`
- **Admin Actions**: `(GameType gameType, AdminActionType actionType)`
- **Ownership**: `(GameType gameType, string? ownerId)` for user-owned resources

## API Client Integration

### Repository API Client Pattern

**ALL data access MUST go through the Repository API Client:**

```csharp
// Example usage
var apiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

if (apiResponse.IsNotFound || apiResponse.Result?.Data is null)
{
    Logger.LogWarning("Resource {ResourceId} not found", id);
    return NotFound();
}

var data = apiResponse.Result.Data;
```

### API Response Validation Pattern

Always validate API responses consistently:

```csharp
if (!apiResponse.IsSuccess || apiResponse.Result?.Data?.Items is null)
{
    Logger.LogError("Failed to retrieve data for user {UserId}", User.XtremeIdiotsId());
    return RedirectToAction("Display", "Errors", new { id = 500 });
}
```

### Helper Methods for Data Retrieval

Create private helper methods for common data retrieval patterns:

```csharp
private async Task<(IActionResult? ActionResult, BanFileMonitorDto? Data)> GetAuthorizedResourceAsync(
    Guid id, 
    string policy, 
    string action, 
    CancellationToken cancellationToken = default)
{
    var apiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitor(id, cancellationToken);

    if (apiResponse.IsNotFound || apiResponse.Result?.Data is null)
    {
        Logger.LogWarning("Resource {ResourceId} not found when {Action}", id, action);
        return (NotFound(), null);
    }

    var data = apiResponse.Result.Data;
    var authResult = await CheckAuthorizationAsync(/*...*/);
    
    return authResult != null ? (authResult, null) : (null, data);
}
```

## Forum Integration

### Admin Action Topics

For admin actions that create forum topics:

```csharp
// Create forum topic for admin action
createDto.ForumTopicId = await adminActionTopics.CreateTopicForAdminAction(
    actionType,
    gameType,
    playerId,
    username,
    DateTime.UtcNow,
    text,
    adminId);
```

### Forum Links in Success Messages

Include forum topic links in success messages:

```csharp
private string CreateActionAppliedMessage(AdminActionType actionType, string username, int? forumTopicId)
{
    var forumBaseUrl = GetForumBaseUrl();
    return $"The {actionType} has been successfully applied against {username} with a <a target=\"_blank\" href=\"{forumBaseUrl}{forumTopicId}-topic/\" class=\"alert-link\">topic</a>";
}
```

## Telemetry and Monitoring

### Success Telemetry Pattern

Use standardized success telemetry for all operations:

```csharp
TrackSuccessTelemetry("BanFileMonitorCreated", "CreateBanFileMonitor", new Dictionary<string, string>
{
    { "GameServerId", model.GameServerId.ToString() },
    { "FilePath", model.FilePath },
    { "GameType", gameServerData.GameType.ToString() }
});
```

### Unauthorized Access Tracking

Unauthorized access attempts are automatically tracked by `CheckAuthorizationAsync`, but for custom scenarios:

```csharp
TrackUnauthorizedAccessAttempt("Edit", "BanFileMonitor", context, additionalData);
```

## ViewData and ViewModels

### ViewData Patterns

Use typed ViewData for dropdown lists and selections:

```csharp
private async Task AddGameServersViewData(Guid? selected = null, CancellationToken cancellationToken = default)
{
    try
    {
        var apiResponse = await repositoryApiClient.GameServers.V1.GetGameServers(/*...*/);
        
        if (apiResponse.Result?.Data?.Items is not null)
        {
            ViewData["GameServers"] = new SelectList(
                apiResponse.Result.Data.Items, 
                nameof(GameServerDto.GameServerId), 
                nameof(GameServerDto.Title), 
                selected);
        }
        else
        {
            ViewData["GameServers"] = new SelectList(
                Enumerable.Empty<GameServerDto>(), 
                nameof(GameServerDto.GameServerId), 
                nameof(GameServerDto.Title));
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error loading view data");
        ViewData["GameServers"] = new SelectList(Enumerable.Empty<GameServerDto>(), "Value", "Text");
    }
}
```

### ViewModel Patterns

ViewModels should include:
- All form fields with proper validation attributes
- Related DTOs for display purposes
- Navigation properties when needed

```csharp
public class CreateBanFileMonitorViewModel
{
    [Required]
    [DisplayName("File Path")]
    public string FilePath { get; set; } = string.Empty;

    [DisplayName("Server")]
    public Guid GameServerId { get; set; }

    [DisplayName("Server")]
    public GameServerDto? GameServer { get; set; }
}
```

## Model Validation

### Async Model Validation

For validation that requires async operations:

```csharp
var modelValidationResult = await CheckModelStateAsync(model, async m =>
{
    await AddGameServersViewData(model.GameServerId, cancellationToken);
    model.GameServer = gameServerData;
});
if (modelValidationResult != null) return modelValidationResult;
```

### Sync Model Validation

For simple validation scenarios:

```csharp
var modelValidationResult = CheckModelState(model, m => m.PlayerDto = playerData);
if (modelValidationResult != null) return modelValidationResult;
```

## Configuration Management

### Configuration Constants

Define default values as constants:

```csharp
private const string DefaultForumBaseUrl = "https://www.xtremeidiots.com/forums/topic/";
private const string DefaultFallbackAdminId = "21145";
private const int DefaultTempBanDurationDays = 7;
```

### Configuration Access

Use BaseController's configuration helper:

```csharp
private string GetForumBaseUrl()
{
    return GetConfigurationValue("AdminActions:ForumBaseUrl", DefaultForumBaseUrl);
}
```

## CRUD Operation Patterns

### Standard CRUD Structure

All CRUD controllers should follow this structure:

1. **Index** - List view with filtering based on user permissions
2. **Details** - Single item view with authorization check
3. **Create GET** - Form view with dropdown population
4. **Create POST** - Form processing with validation and authorization
5. **Edit GET** - Edit form with pre-populated data
6. **Edit POST** - Update processing with validation and authorization
7. **Delete GET** - Confirmation view
8. **Delete POST** - Actual deletion with authorization

### Consistent Action Naming

- Use `DeleteConfirmed` for POST delete actions
- Use `[ActionName("Delete")]` attribute for POST methods
- Always include `[ValidateAntiForgeryToken]` on POST actions

```csharp
[HttpPost]
[ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken = default)
```

## User Context and Claims

### User Identification

Always use the extension methods for user identification:

```csharp
var userId = User.XtremeIdiotsId();
var username = User.Username();
var email = User.Email();
```

### Claim-Based Filtering

Use claims to filter data based on user permissions:

```csharp
var requiredClaims = new[] { UserProfileClaimType.SeniorAdmin, UserProfileClaimType.HeadAdmin };
var (gameTypes, itemIds) = User.ClaimedGamesAndItems(requiredClaims);

var apiResponse = await repositoryApiClient.SomeResource.V1.GetItems(
    gameTypes, itemIds, /*other params*/, cancellationToken);
```

## Redirects and Navigation

### Success Redirects

After successful operations, redirect to appropriate views:

```csharp
// Redirect to index after creation/update/deletion
return RedirectToAction(nameof(Index));

// Redirect to details view after creation
return RedirectToAction("Details", "Players", new { id = model.PlayerId });
```

### Error Handling Redirects

For API failures, redirect to error pages:

```csharp
if (!apiResponse.IsSuccess)
{
    Logger.LogError("API operation failed for user {UserId}", User.XtremeIdiotsId());
    return RedirectToAction("Display", "Errors", new { id = 500 });
}
```

## Alert Messages

### Success Messages

Use the extension methods for consistent alert styling:

```csharp
this.AddAlertSuccess($"The ban file monitor has been created for {gameServerData.Title}");
this.AddAlertDanger("An error occurred while processing your request. Please try again.");
```

### Message Formatting

Include relevant context in messages:

```csharp
private static string CreateActionOperationMessage(AdminActionType actionType, string username, string operation)
{
    return $"The {actionType} has been successfully {operation} for {username}";
}
```

## Performance Considerations

### Cancellation Token Usage

**Always** pass cancellation tokens through the call chain:

```csharp
public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
{
    return await ExecuteWithErrorHandlingAsync(async () =>
    {
        var result = await repositoryApiClient.SomeMethod(cancellationToken);
        // ...
    }, "ActionDescription");
}
```

### Pagination

For list views, implement proper pagination:

```csharp
var apiResponse = await repositoryApiClient.BanFileMonitors.V1.GetBanFileMonitors(
    gameTypes, itemIds, searchTerm, skip: 0, take: 50, orderBy, cancellationToken);
```

## Security Best Practices

### Anti-Forgery Tokens

**Always** include anti-forgery tokens on POST actions:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateViewModel model, CancellationToken cancellationToken = default)
```

### Input Validation

- Use Data Annotations for basic validation
- Perform additional business logic validation in controllers
- Always validate authorization before processing

### Sensitive Data Logging

Be careful not to log sensitive information:

```csharp
// DON'T log passwords, API keys, or sensitive user data
Logger.LogInformation("User {UserId} performed action on resource {ResourceId}", 
    User.XtremeIdiotsId(), resourceId);
```

## Testing Considerations

### Controller Testability

Design controllers to be easily testable:
- Use dependency injection for all dependencies
- Keep business logic in separate services when complex
- Use the BaseController helpers consistently

### Logging for Debugging

Include appropriate logging for debugging:

```csharp
Logger.LogInformation("User {UserId} attempting to {Action} resource {ResourceId}",
    User.XtremeIdiotsId(), "create", resourceId);
```

## Common Anti-Patterns to Avoid

❌ **Direct database access in controllers**
❌ **Synchronous calls in async methods** (e.g., `.Wait()`, `.Result`)
❌ **Missing authorization checks on sensitive operations**
❌ **Inconsistent error handling patterns**
❌ **Manual telemetry creation instead of using BaseController helpers**
❌ **Not validating API responses**
❌ **Missing cancellation token propagation**
❌ **Hard-coded configuration values**

## Integration Patterns

### Forums Integration

For operations that create or update forum content:

```csharp
// Always check if forum integration is available
if (adminActionData.ForumTopicId.HasValue && adminActionData.ForumTopicId != 0)
{
    await adminActionTopics.UpdateTopicForAdminAction(/*...*/);
}
```

### External Service Resilience

Handle external service failures gracefully:

```csharp
try
{
    var result = await externalService.CallAsync();
    return result;
}
catch (Exception ex)
{
    Logger.LogWarning(ex, "External service unavailable, continuing with fallback");
    return fallbackValue;
}
```

## Documentation Requirements

### XML Documentation

**All public controller methods MUST have XML documentation:**
- Include `<summary>` describing the action's purpose
- Document all `<param>` including their business meaning
- Describe `<returns>` including redirect scenarios and error conditions  
- List `<exception>` types that can be thrown for authorization or validation failures

**Private helper methods should have XML docs only if they contain complex business logic.**
