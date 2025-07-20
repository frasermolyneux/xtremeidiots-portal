---
description: 'Guidelines for building Razor views (.cshtml files) in the XtremeIdiots Portal'
applyTo: '**/*.cshtml'
---

# Razor Views (.cshtml) Guidelines

> **IMPORTANT**: All C# formatting and style decisions within Razor views must follow the project's `.editorconfig` file as the authoritative source of truth.

## Core Principles

**Build accessible, responsive, and maintainable Razor views following ASP.NET Core MVC best practices with gaming community-specific patterns.**

## View Structure and Organization

### File Organization
```
Views/
├── Shared/
│   ├── _Layout.cshtml           # Main layout
│   ├── _ViewImports.cshtml      # Global imports and tag helpers
│   ├── _ViewStart.cshtml        # Default layout assignment
│   ├── Components/              # ViewComponent templates
│   │   └── {ComponentName}/
│   │       └── Default.cshtml
│   └── Partials/               # Reusable partial views
└── {ControllerName}/           # Controller-specific views
    ├── Index.cshtml
    ├── Details.cshtml
    └── Create.cshtml
```

### ViewImports Pattern
Always include necessary imports at the top of views:
```csharp
@using Microsoft.AspNetCore.Authorization
@using XtremeIdiots.Portal.Web.Auth.Constants
@using XtremeIdiots.Portal.Web.Extensions
@using XtremeIdiots.Portal.Repository.Abstractions.Constants.V1
@inject IAuthorizationService AuthorizationService
```

## View Models and Type Safety

### Strong Typing
**ALWAYS** use strongly-typed views with ViewModels:
```csharp
@model PlayerDetailsViewModel

@{
    ViewData["Title"] = (Model?.Player?.Username ?? "Unknown Player") + " Player Details";
}
```

### Null Safety
Handle null values defensively:
```csharp
// ✅ Good: Safe navigation and fallback
@(Model?.Player?.Username ?? "Unknown Player")

// ❌ Bad: Potential null reference
@Model.Player.Username
```

### ViewData Usage
Use ViewData for page metadata only:
```csharp
@{
    ViewData["Title"] = "Page Title";
    ViewData["Description"] = "Page description for SEO";
}
```

## Authorization Integration

### Policy-Based Authorization
Use the custom policy tag helper for conditional rendering:
```csharp
<div policy="@AuthPolicies.AccessPlayers">
    <!-- Content only visible to authorized users -->
</div>
```

### Authorization Service Injection
For complex authorization logic, inject the authorization service:
```csharp
@inject IAuthorizationService AuthorizationService

@if ((await AuthorizationService.AuthorizeAsync(User, AuthPolicies.CreateAdminAction)).Succeeded)
{
    <a asp-action="Create" class="btn btn-primary">Create Action</a>
}
```

## UI Framework and Styling

### Bootstrap Integration
Follow Bootstrap 4 patterns with custom Inspinia theme:
```html
<div class="wrapper wrapper-content animated fadeInRight">
    <div class="row">
        <div class="col-12">
            <div class="ibox">
                <div class="ibox-title">
                    <h5>Section Title</h5>
                </div>
                <div class="ibox-content">
                    <!-- Main content -->
                </div>
            </div>
        </div>
    </div>
</div>
```

### Responsive Design
Always implement mobile-first responsive patterns:
```html
<div class="d-flex flex-column flex-md-row align-items-start align-items-md-center">
    <div class="mr-0 mr-md-3 mb-2 mb-md-0 text-center text-md-left">
        <!-- Icon content -->
    </div>
    <div class="flex-grow-1">
        <!-- Main content -->
    </div>
</div>
```

### CSS Classes Pattern
Use semantic CSS classes with consistent naming:
```html
<!-- Gaming-specific styling -->
<span class="game-server-status online">Online</span>
<div class="player-details-banner">
<span class="admin-action-type ban">Ban</span>
```

## HTML Extensions and Helpers

### Game-Specific Extensions
Use custom HTML extensions for gaming content:
```csharp
// Game type icons
@Html.GameTypeIcon(player.GameType)

// Player links with GUID formatting
@Html.GuidLink(player.Guid, player.GameType.ToString())

// IP address links with geolocation
@Html.IPAddressLink(ipAddress)

// Confidence score formatting
@Html.ConfidenceScore(score)
```

### DateTime Formatting
Use standardized datetime extensions:
```csharp
@Html.DateTime(dateValue)
@Html.TimeAgo(dateValue)
```

## Form Patterns

### ASP.NET Core Tag Helpers
Use tag helpers consistently for forms:
```html
<form asp-action="Create" asp-controller="AdminActions">
    <input type="hidden" asp-for="PlayerId" />
    <div class="form-group">
        <label asp-for="Text"></label>
        <textarea asp-for="Text" class="form-control" rows="4"></textarea>
        <span asp-validation-for="Text" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Submit</button>
</form>
```

### Validation
Include validation scripts in forms:
```html
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### Rich Text Editing
Use Summernote for rich text content:
```html
<textarea id="summernote" asp-for="Text" class="form-control"></textarea>

@section Scripts {
    <script>
        $('#summernote').summernote({
            height: 200,
            toolbar: [
                ['style', ['bold', 'italic', 'underline']],
                ['para', ['ul', 'ol']],
                ['misc', ['codeview']]
            ]
        });
    </script>
}
```

## ViewComponents Integration

### Invoking ViewComponents
Use ViewComponents for reusable UI sections:
```csharp
@await Component.InvokeAsync("PlayerTags", playerId)
@await Component.InvokeAsync("GameServerList", new { gameType = GameType.CallOfDuty4 })
@await Component.InvokeAsync("AdminActions", new { playerId = Model.Player.PlayerId })
```

### ViewComponent Templates
Structure ViewComponent templates consistently:
```csharp
@model List<PlayerTagDto>

<div class="panel-body">
    <h5>Component Title</h5>
    
    @if (Model.Any())
    {
        @foreach (var item in Model)
        {
            <!-- Item template -->
        }
    }
    else
    {
        <p class="text-muted">No items found.</p>
    }
</div>
```

## Performance and Loading

### Conditional Script Loading
Load scripts conditionally based on content:
```html
@section Scripts {
    @if (IsSectionDefined("DataTables"))
    {
        <script src="~/lib/datatables/js/jquery.dataTables.min.js"></script>
    }
}
```

### Environment-Specific Resources
Use environment tag helpers for optimized loading:
```html
<environment names="Development">
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.css" />
</environment>
<environment names="Staging,Production">
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
</environment>
```

## Data Display Patterns

### Tables and Lists
Use consistent table patterns for data display:
```html
<div class="table-responsive">
    <table class="table table-striped table-hover">
        <thead>
            <tr>
                <th>Column 1</th>
                <th>Column 2</th>
                <th class="text-center">Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model.Items)
            {
                <tr>
                    <td>@item.Property1</td>
                    <td>@item.Property2</td>
                    <td class="text-center">
                        <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-outline-primary">
                            <i class="fa fa-eye"></i>
                        </a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

### DataTables Integration
For large datasets, use DataTables with AJAX:
```html
<table id="dataTable" class="table table-striped table-hover">
    <!-- Table structure -->
</table>

@section Scripts {
    <script>
        $('#dataTable').DataTable({
            "ajax": {
                "url": "@Url.Action("GetData")",
                "type": "POST"
            },
            "columns": [
                { "data": "property1" },
                { "data": "property2" },
                { "data": null, "defaultContent": "<button class='btn btn-sm btn-primary'>Action</button>" }
            ],
            "order": [[0, "desc"]],
            "pageLength": 25
        });
    </script>
}
```

## Error Handling and User Feedback

### Alert Messages
Use the custom alerts system:
```html
<alerts></alerts> <!-- Custom tag helper for displaying alerts -->
```

### Loading States
Implement loading indicators for AJAX operations:
```html
<div id="loadingIndicator" class="text-center" style="display: none;">
    <i class="fa fa-spinner fa-spin"></i> Loading...
</div>
```

### Empty States
Provide meaningful empty state messages:
```html
@if (!Model.Items.Any())
{
    <div class="text-center text-muted p-4">
        <i class="fa fa-info-circle fa-2x mb-2"></i>
        <p>No items found. <a asp-action="Create">Create one now</a>.</p>
    </div>
}
```

## SEO and Accessibility

### Meta Tags
Include appropriate meta tags:
```html
@{
    ViewData["Title"] = "Descriptive Page Title";
    ViewData["Description"] = "Page description for search engines";
}
```

### Semantic HTML
Use semantic HTML elements:
```html
<main role="main">
    <section aria-labelledby="main-heading">
        <h1 id="main-heading">Page Title</h1>
        <!-- Content -->
    </section>
</main>
```

### Accessibility
Ensure proper ARIA attributes and keyboard navigation:
```html
<button type="button" 
        class="btn btn-primary" 
        aria-label="Edit player details"
        data-toggle="modal" 
        data-target="#editModal">
    <i class="fa fa-edit" aria-hidden="true"></i>
    Edit
</button>
```

## Gaming-Specific Patterns

### Game Server Status
Display server status with consistent styling:
```html
<span class="badge badge-@(server.IsOnline ? "success" : "danger")">
    @(server.IsOnline ? "Online" : "Offline")
</span>
```

### Player Information Display
Use consistent patterns for player data:
```html
<div class="player-info">
    <div class="d-flex align-items-center">
        @Html.GameTypeIcon(player.GameType)
        <span class="ml-2 font-weight-bold">@player.Username</span>
    </div>
    <small class="text-muted">
        ID: @player.PlayerId | GUID: @Html.GuidLink(player.Guid, player.GameType.ToString())
    </small>
</div>
```

### Admin Actions
Style admin actions consistently:
```html
<div class="admin-action admin-action-@adminAction.Type.ToString().ToLower()">
    <div class="d-flex justify-content-between align-items-start">
        <div>
            <strong>@adminAction.Type</strong>
            <span class="text-muted">by @adminAction.Username</span>
        </div>
        <small class="text-muted">@Html.TimeAgo(adminAction.Created)</small>
    </div>
    <div class="mt-2">
        @Html.Raw(adminAction.Text)
    </div>
</div>
```

## Security Considerations

### XSS Prevention
Always use appropriate encoding:
```csharp
// ✅ Good: Automatic HTML encoding
@Model.UserInput

// ✅ Good: Explicit HTML encoding
@Html.Encode(Model.UserInput)

// ⚠️ Caution: Only for trusted HTML content
@Html.Raw(Model.TrustedHtmlContent)
```

### Form Security
Include anti-forgery tokens:
```html
<form asp-action="Create">
    @Html.AntiForgeryToken()
    <!-- Form fields -->
</form>
```

## Performance Best Practices

### Minimize Database Calls
Use ViewModels to aggregate data in controllers, not views:
```csharp
// ✅ Good: Data aggregated in controller
@model PlayerDetailsViewModel // Contains all needed data

// ❌ Bad: Multiple service calls in view
@await SomeService.GetDataAsync()
```

### Conditional Rendering
Use conditional compilation for expensive operations:
```csharp
@if (Model.ShowExpensiveContent)
{
    @await Component.InvokeAsync("ExpensiveComponent")
}
```

## Quality Checklist

Before committing Razor views, ensure:
- [ ] Strong typing with appropriate ViewModel
- [ ] Null safety and defensive programming
- [ ] Proper authorization checks using policies
- [ ] Responsive design with Bootstrap classes
- [ ] Semantic HTML with accessibility attributes
- [ ] Consistent styling following project patterns
- [ ] XSS protection and proper encoding
- [ ] Performance considerations (minimal database calls)
- [ ] SEO-friendly meta tags and structure
- [ ] Gaming-specific UI patterns where applicable

## Anti-Patterns to Avoid

### Business Logic in Views
```csharp
// ❌ Bad: Business logic in view
@{
    var calculatedValue = Model.Price * 0.1 + SomeComplexCalculation();
}

// ✅ Good: Calculation in ViewModel property
@Model.CalculatedValue
```

### Inline Styles
```html
<!-- ❌ Bad: Inline styles -->
<div style="color: red; font-size: 14px;">Content</div>

<!-- ✅ Good: CSS classes -->
<div class="text-danger small">Content</div>
```

### Deep Nesting
```html
<!-- ❌ Bad: Excessive nesting -->
@if (condition1)
{
    @if (condition2)
    {
        @if (condition3)
        {
            <!-- Content -->
        }
    }
}

<!-- ✅ Good: Early returns or combined conditions -->
@if (condition1 && condition2 && condition3)
{
    <!-- Content -->
}
```

## Testing Considerations

### View Testing
Ensure views can be unit tested:
- Use ViewModels that can be easily mocked
- Minimize dependencies on external services
- Keep complex logic in controllers or services

### Integration Testing
Test complete user workflows:
- Form submissions with validation
- Authorization scenarios
- AJAX interactions
- Responsive behavior

### Automation Testing & Element Identification

**ALL interactive elements must include automation IDs for end-to-end testing.**

#### Automation ID Naming Convention
Use the `data-testid` attribute with a consistent kebab-case naming pattern:

```
data-testid="{page-context}-{element-type}-{action/purpose}"
```

#### Common Element Patterns

**Buttons:**
```html
<!-- Primary actions -->
<button type="submit" class="btn btn-primary" data-testid="player-create-submit">
    Create Player
</button>

<!-- Secondary actions -->
<a class="btn btn-secondary" asp-action="Index" data-testid="player-details-back">
    Back to List
</a>

<!-- Action buttons in tables/lists -->
<a class="btn btn-sm btn-primary" asp-action="Edit" asp-route-id="@item.Id" 
   data-testid="player-@(item.Id)-edit">
    Edit
</a>
```

**Form Controls:**
```html
<!-- Input fields -->
<input asp-for="Username" class="form-control" 
       data-testid="player-form-username" />

<!-- Select dropdowns -->
<select asp-for="GameType" class="form-control" 
        data-testid="player-form-gametype">
    <!-- Options -->
</select>

<!-- Checkboxes -->
<input type="checkbox" asp-for="IsActive" 
       data-testid="player-form-isactive" />

<!-- Text areas -->
<textarea asp-for="Reason" class="form-control" 
          data-testid="admin-action-form-reason"></textarea>
```

**Navigation & Links:**
```html
<!-- Main navigation -->
<a asp-controller="Players" asp-action="Index" 
   data-testid="nav-players">Players</a>

<!-- Breadcrumb links -->
<a asp-action="Details" asp-route-id="@Model.PlayerId" 
   data-testid="breadcrumb-player-details">@Model.Player.Username</a>

<!-- Tab navigation -->
<a class="nav-link" href="#admin-actions" 
   data-testid="player-tab-admin-actions">Admin Actions</a>
```

**Tables & Data Display:**
```html
<!-- Table headers (for sorting) -->
<th>
    <a asp-action="Index" asp-route-sortOrder="@ViewData["UsernameSort"]"
       data-testid="players-table-sort-username">
        Username
    </a>
</th>

<!-- Table row actions -->
<td>
    <div class="btn-group" role="group">
        <a class="btn btn-sm btn-primary" asp-action="Details" 
           asp-route-id="@item.PlayerId"
           data-testid="player-@(item.PlayerId)-details">
            Details
        </a>
        <a class="btn btn-sm btn-warning" asp-action="Edit" 
           asp-route-id="@item.PlayerId"
           data-testid="player-@(item.PlayerId)-edit">
            Edit
        </a>
    </div>
</td>
```

**Modals & Dialogs:**
```html
<!-- Modal triggers -->
<button type="button" class="btn btn-danger" data-toggle="modal" 
        data-target="#deleteModal" 
        data-testid="admin-action-delete-trigger">
    Delete
</button>

<!-- Modal actions -->
<div class="modal" id="deleteModal">
    <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal"
                data-testid="delete-modal-cancel">
            Cancel
        </button>
        <button type="button" class="btn btn-danger" 
                data-testid="delete-modal-confirm">
            Delete
        </button>
    </div>
</div>
```

**Search & Filters:**
```html
<!-- Search inputs -->
<input type="text" class="form-control" placeholder="Search players..." 
       data-testid="players-search-input" />

<!-- Filter dropdowns -->
<select class="form-control" data-testid="players-filter-gametype">
    <option value="">All Games</option>
    <!-- Options -->
</select>

<!-- Apply filters button -->
<button type="submit" class="btn btn-primary" 
        data-testid="players-filter-apply">
    Apply Filters
</button>
```

**Status Indicators & Badges:**
```html
<!-- Status badges -->
<span class="badge badge-success" 
      data-testid="player-@(item.PlayerId)-status">
    Active
</span>

<!-- Gaming-specific indicators -->
<span class="game-server-status online" 
      data-testid="server-@(server.Id)-status">
    Online
</span>
```

#### Context-Specific Patterns

**Controller/Page Context Examples:**
- `player-details-*` - Player details page elements
- `admin-actions-*` - Admin actions related elements
- `server-list-*` - Server listing page elements
- `ban-file-monitor-*` - Ban file monitor elements

**Action/Purpose Examples:**
- `*-submit` - Form submission buttons
- `*-cancel` - Cancel/back actions
- `*-edit` - Edit action links/buttons
- `*-delete` - Delete action elements
- `*-create` - Create/add new elements
- `*-search` - Search functionality
- `*-filter` - Filter controls
- `*-sort` - Sorting controls

#### Testing Best Practices

1. **Uniqueness:** Each automation ID should be unique within the page
2. **Stability:** IDs should remain stable across code changes
3. **Meaningful:** IDs should clearly indicate the element's purpose
4. **Hierarchical:** Use context to create logical groupings
5. **Dynamic IDs:** For repeated elements (like table rows), include unique identifiers:
   ```html
   data-testid="player-@(item.PlayerId)-edit"
   ```

#### ViewComponent Testing
ViewComponents should also include automation IDs:
```html
<!-- In ViewComponent templates -->
<div class="player-tags-component" data-testid="player-tags-component">
    @foreach (var tag in Model)
    {
        <span class="badge" data-testid="player-tag-@(tag.TagId)">
            @tag.Name
        </span>
    }
</div>
```

## Summary

Razor views in the XtremeIdiots Portal should be **strongly-typed, secure, accessible, and follow gaming community UI patterns**. Focus on creating reusable components, maintaining consistent styling, and ensuring excellent user experience across all devices.
