---
mode: agent
---

# Controller Modernization and Standardization

Modernize and standardize the controller to follow the established patterns and best practices demonstrated in the AdminActionsController.cs. Ensure consistency across all controllers in the XtremeIdiots Portal Web application.

## Requirements

### 1. Naming Conventions and Structure
- **Controller Class**: Use PascalCase with descriptive names (e.g., `AdminActionController`, `PlayersController`)
- **Action Methods**: Use PascalCase verbs describing the action (e.g., `Create`, `Edit`, `Delete`, `Details`)
- **Parameters**: Use camelCase for method parameters (e.g., `id`, `cancellationToken`, `model`)
- **Private Fields**: Use camelCase for dependency fields (avoid `this.` prefix unless required for disambiguation)
- **Local Variables**: Use camelCase with descriptive names (e.g., `playerData`, `adminActionData`)

### 2. Dependency Injection Pattern
- Inject all required services through constructor
- Use null checks with `ArgumentNullException` for each dependency (use `this.` prefix in constructor for disambiguation)
- Store dependencies as `private readonly` fields
- Required dependencies typically include:
  - `IAuthorizationService authorizationService`
  - `IRepositoryApiClient repositoryApiClient`
  - `TelemetryClient telemetryClient`
  - `ILogger<ControllerName> logger`

### 3. Method Signatures and Attributes
- All async methods must accept `CancellationToken cancellationToken = default`
- Use appropriate HTTP method attributes (`[HttpGet]`, `[HttpPost]`)
- Add `[ValidateAntiForgeryToken]` to POST actions
- Use `[ActionName("ActionName")]` when method name differs from action name
- Add comprehensive XML documentation for all public methods

### 4. Authorization Pattern
- Apply `[Authorize(Policy = AuthPolicies.PolicyName)]` at controller level for broad access control
- Use `AuthPolicies` constants class for all policy names (e.g., `AuthPolicies.AccessAdminActionsController`)
- Each logical action grouping should have corresponding AuthHandler that implements `IAuthorizationHandler`
- Perform granular authorization checks within actions using `authorizationService.AuthorizeAsync(User, resource, AuthPolicies.PolicyName)`
- Create authorization resources as tuples when multiple parameters needed (e.g., `(gameType, adminActionType)` or `(gameType, adminId)`)
- Authorization handlers check user claims against requirements and call `context.Succeed(requirement)` when authorized
- Each controller area should have dedicated policies: Access (controller-level), Create, Edit, Delete, View, etc.
- Log authorization failures with appropriate warning messages
- Track authorization denials with telemetry events

### 5. AuthPolicies and AuthHandler Implementation
- **AuthPolicies Constants**: Add new policy constants to `Auth/Constants/AuthPolicies.cs` organized by functional area
  - Use consistent naming: `Access[ControllerName]`, `Create[Entity]`, `Edit[Entity]`, `Delete[Entity]`, `View[Entity]`
  - Group related policies with comments (e.g., `// Admin Actions`, `// Game Servers`, `// Players`)
- **Authorization Requirements**: Create requirement classes in `Auth/Requirements/` that implement `IAuthorizationRequirement`
  - Keep requirements simple marker interfaces unless complex logic needed
  - Name requirements to match policy names (e.g., `AccessAdminActions`, `CreateAdminAction`)
- **Authorization Handlers**: Create handler classes in `Auth/Handlers/` that implement `IAuthorizationHandler`
  - Use pattern: `[ControllerName]AuthHandler` (e.g., `AdminActionsAuthHandler`, `PlayersAuthHandler`)
  - Implement `HandleAsync()` method that checks `context.PendingRequirements` and delegates to specific methods
  - Create private methods for each requirement type (e.g., `HandleAccessAdminActions()`, `HandleCreateAdminAction()`)
  - Check user claims using `context.User.HasClaim()` and call `context.Succeed(requirement)` when authorized
  - Support contextual resources passed via `context.Resource` for game-specific or entity-specific permissions
- **Policy Registration**: Register policies in `Extensions/PolicyExtensions.cs` using `options.AddPolicy()`
  - Connect each policy name to its corresponding requirement class
  - Group registrations by functional area with comments

### 6. Logging Standards
- **Information Level**: Action entry, successful operations, and business flow
  - Format: `"User {UserId} attempting to [action] [resource] {ResourceId}"`
  - Format: `"User {UserId} successfully [completed action] [resource] {ResourceId}"`
- **Warning Level**: Authorization failures, not found scenarios, validation failures
  - Format: `"User {UserId} denied access to [action] [resource] {ResourceId}"`
  - Format: `"[Resource] {ResourceId} not found when [performing action]"`
- **Error Level**: Exceptions and unexpected failures
  - Format: `"Error [performing action] [resource] {ResourceId}"`

### 7. Error Handling and Exception Management
- Wrap all action methods in try-catch blocks
- Log exceptions with structured logging including relevant IDs
- Create `ExceptionTelemetry` with `SeverityLevel.Error`
- Add relevant properties to telemetry (UserId, ResourceIds, ActionType)
- For GET actions: Re-throw exceptions to show error pages
- For POST actions: Add danger alerts and return to appropriate view/redirect

### 8. Telemetry and Audit Trail
- **Custom Events (Limited Use)**: Use custom telemetry events ONLY for:
  - **User Interaction Events**: Significant user actions that need business analytics (e.g., `"AdminActionCreated"`, `"PlayerBanned"`, `"DemoUploaded"`)
  - **Security Events**: Use the standardized `"UnauthorizedUserAccessAttempt"` event for ALL authorization failures across controllers
  - **System Flow Events**: Critical business process tracking (e.g., `"ForumTopicCreated"`, `"BanFileProcessed"`)
- **Standard ILogger (Primary Use)**: Use `ILogger` for all other telemetry and logging:
  - Action entry/exit logging
  - Authorization failures and access denials (alongside the security event)
  - Data retrieval operations
  - Validation failures
  - Error handling and exceptions
  - Performance and diagnostic information
- **Unified Security Event Pattern**: For ALL authorization failures, use:
  ```csharp
  var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
      .Enrich(User);
  unauthorizedTelemetry.Properties.TryAdd("Controller", "ControllerName");
  unauthorizedTelemetry.Properties.TryAdd("Action", "ActionName");
  unauthorizedTelemetry.Properties.TryAdd("Resource", "ResourceType");
  unauthorizedTelemetry.Properties.TryAdd("Context", "SpecificContext");
  // Add additional context-specific properties as needed
  telemetryClient.TrackEvent(unauthorizedTelemetry);
  ```
- **CRITICAL: Legacy Security Event Cleanup**: Replace ALL existing authorization denial events with the unified pattern, including but not limited to:
  - `"AdminActionCreateDenied"` → `"UnauthorizedUserAccessAttempt"`
  - `"AdminActionEditDenied"` → `"UnauthorizedUserAccessAttempt"`
  - `"AdminActionDeleteDenied"` → `"UnauthorizedUserAccessAttempt"`
  - `"AdminActionLiftDenied"` → `"UnauthorizedUserAccessAttempt"`
  - `"AdminActionClaimDenied"` → `"UnauthorizedUserAccessAttempt"`
  - `"AdminActionTopicCreateDenied"` → `"UnauthorizedUserAccessAttempt"`
  - ANY event ending in "Denied" or "AccessDenied" → `"UnauthorizedUserAccessAttempt"`
  - Use regex search for `new EventTelemetry\(".*Denied.*"\)` to find all instances
- **TelemetryExtensions Usage (Custom Events Only)**: When creating custom events, leverage the established enrichment pattern
  - `eventTelemetry.Enrich(User)` - adds logged-in user context
  - `eventTelemetry.Enrich(playerDto)` - adds PlayerId
  - `eventTelemetry.Enrich(adminActionDto)` - adds PlayerId and AdminActionType
  - `eventTelemetry.Enrich(gameServerDto)` - adds GameServerId
  - `eventTelemetry.Enrich(demoDto)` - adds DemoId, GameType, and DemoTitle
  - **Chain Pattern**: `eventTelemetry.Enrich(User).Enrich(playerData).Enrich(actionData)`
- **Event Naming**: Use PascalCase descriptive names for custom events (e.g., `"AdminActionCreated"`, `"PlayerBanned"`)
- **Guideline**: If in doubt, use `ILogger` - only create custom events for business-critical user interactions and system flows that require dedicated analytics tracking

### 9. Data Validation and Model State
- Check `ModelState.IsValid` before processing POST requests
- Return view with model when validation fails
- Reload necessary data for view model when returning with validation errors
- Use appropriate HTTP status codes (NotFound, BadRequest, Unauthorized)

### 10. API Client Integration
- Use proper null checking for API responses (`IsNotFound`, `Result?.Data`)
- Handle both not found and null data scenarios
- Use appropriate entity options when fetching data
- Log warnings for data not found scenarios

### 11. User Feedback and Alerts
- Use `AddAlertSuccess()` for successful operations
- Use `AddAlertDanger()` for error scenarios
- Include relevant entity names and actions in alert messages
- Provide helpful links in success messages when applicable

### 12. View Model Patterns
- Create strongly-typed view models for complex scenarios
- Populate all required properties for views
- Reload data when returning views due to validation errors
- Use DTOs for API communication and view models for UI

### 13. Forum Integration (if applicable)
- Create forum topics for relevant actions
- Update forum topics when modifying related entities
- Store forum topic IDs in entity data
- Handle forum integration failures gracefully

### 14. Unified Security Event Pattern
- **Single Event Type**: Use `"UnauthorizedUserAccessAttempt"` for ALL authorization failures across all controllers
- **Context Through Properties**: Differentiate events using additional properties rather than different event names:
  - `Controller` - Name of the controller (e.g., "AdminActions", "BanFileMonitors", "Demos")
  - `Action` - Name of the action being attempted (e.g., "Create", "Edit", "Delete", "View")
  - `Resource` - Type of resource being accessed (e.g., "AdminAction", "BanFileMonitor", "Demo")
  - `Context` - Specific contextual information (e.g., "GameType:CallOfDuty,AdminActionType:Ban")
- **Standard Pattern**: Always enrich with user context and relevant entity data
- **Consistent Implementation**: This approach enables unified security analytics while maintaining detailed context
- **Example for AdminActions Create**:
  ```csharp
  var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
      .Enrich(User)
      .Enrich(playerData);
  unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
  unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
  unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
  unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{playerData.GameType},AdminActionType:{adminActionType}");
  telemetryClient.TrackEvent(unauthorizedTelemetry);
  ```

### 15. TelemetryExtensions Pattern Implementation (Custom Events Only)
- **Limited Scope**: Use TelemetryExtensions and custom events ONLY for significant user interactions and system flow events
- **Primary Logging**: Use standard `ILogger` for routine operations, diagnostics, errors, and authorization events
- **Extension Method Pattern**: When creating custom events, use the `TelemetryExtensions` class to enrich `EventTelemetry` objects
- **Fluent Interface**: Chain multiple `.Enrich()` calls for comprehensive telemetry context
- **Automatic Property Addition**: Extensions automatically add relevant properties using `TryAdd()` to avoid duplicates
- **Standard Enrichment Methods**:
  - `.Enrich(ClaimsPrincipal)` - adds LoggedInAdminId and LoggedInUsername
  - `.Enrich(PlayerDto)` - adds PlayerId
  - `.Enrich(AdminActionDto)` - adds PlayerId and AdminActionType  
  - `.Enrich(CreateAdminActionDto)` - adds PlayerId and AdminActionType
  - `.Enrich(EditAdminActionDto)` - adds AdminActionId
  - `.Enrich(GameServerDto)` - adds GameServerId
  - `.Enrich(BanFileMonitorDto)` - adds BanFileMonitorId and GameServerId
  - `.Enrich(DemoDto)` - adds DemoId, GameType, and DemoTitle
- **Example Usage Pattern** (for business-critical events only):
  ```csharp
  // Only for significant user interactions
  var eventTelemetry = new EventTelemetry("AdminActionCreated");
  eventTelemetry
      .Enrich(User)
      .Enrich(playerData)
      .Enrich(createAdminActionDto);
  telemetryClient.TrackEvent(eventTelemetry);
  
  // For authorization failures, use unified security event
  var unauthorizedTelemetry = new EventTelemetry("UnauthorizedUserAccessAttempt")
      .Enrich(User)
      .Enrich(playerData);  // or other relevant entity
  unauthorizedTelemetry.Properties.TryAdd("Controller", "AdminActions");
  unauthorizedTelemetry.Properties.TryAdd("Action", "Create");
  unauthorizedTelemetry.Properties.TryAdd("Resource", "AdminAction");
  unauthorizedTelemetry.Properties.TryAdd("Context", $"GameType:{gameType},AdminActionType:{adminActionType}");
  telemetryClient.TrackEvent(unauthorizedTelemetry);
  
  // For routine operations, use ILogger instead:
  logger.LogInformation("User {UserId} accessed admin actions for player {PlayerId}", 
      User.XtremeIdiotsId(), playerId);
  ```
- **Extension Guidelines**: When creating new enrichment methods, follow the established pattern:
  - Return the EventTelemetry object for method chaining
  - Use `TryAdd()` to prevent property conflicts
  - Include the most relevant identifiers for the entity type
  - Use consistent property naming (e.g., "PlayerId", "GameServerId")
- **Decision Criteria**: Create custom events only when you need dedicated business analytics for user behavior or critical system processes

## Success Criteria

✅ **Consistent Naming**: All classes, methods, and variables follow established conventions  
✅ **Comprehensive Logging**: All actions log entry, success, warnings, and errors appropriately  
✅ **Robust Authorization**: All actions properly check permissions and log failures  
✅ **Unified Security Events**: All authorization failures use the standardized `"UnauthorizedUserAccessAttempt"` event with proper context  
✅ **Legacy Event Cleanup**: NO legacy authorization denial events remain (verified with regex search)  
✅ **Complete Telemetry**: All operations tracked using TelemetryExtensions enrichment pattern  
✅ **Proper Error Handling**: All exceptions caught, logged, and handled appropriately  
✅ **User Experience**: Clear feedback provided for all user actions  
✅ **Code Documentation**: All public methods have comprehensive XML documentation  
✅ **Security**: All POST actions protected with anti-forgery tokens  
✅ **Performance**: Efficient data loading with appropriate entity options  
✅ **Maintainability**: Consistent patterns make code easy to understand and modify

## Implementation Notes

- Follow the patterns established in `AdminActionsController.cs` as the reference implementation
- Ensure all logging uses structured logging with proper parameter placeholders
- Use the extension method `User.XtremeIdiotsId()` to get current user ID consistently
- Implement cancellation token support throughout the async call chain
- Consider business logic requirements when determining authorization resources
- **MANDATORY: Legacy Security Event Audit**: Before completing modernization, perform a comprehensive search for ALL legacy authorization denial events:
  1. Search for `new EventTelemetry("` to find all custom events
  2. Search for `.*Denied` or `.*AccessDenied` event names
  3. Replace ALL authorization denial events with `"UnauthorizedUserAccessAttempt"`
  4. Verify no legacy denial events remain using regex: `new EventTelemetry\(".*Denied.*"\)`
- **Telemetry Strategy**: Use `ILogger` for routine operations and diagnostics; reserve custom events with `TelemetryExtensions` for significant user interactions and system flows only
- **Security Event Strategy**: Use the unified `"UnauthorizedUserAccessAttempt"` event for ALL authorization failures with context in additional properties
- **Custom Event Criteria**: Only create custom telemetry events for business-critical actions that require dedicated analytics tracking
- **Security Event Context**: Always include Controller, Action, Resource, and Context properties for unauthorized access events
- Leverage the fluent interface pattern for telemetry enrichment when using custom events: `.Enrich(User).Enrich(entityData)`
- Test all error scenarios to ensure proper logging and user feedback