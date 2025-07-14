---
applyTo: '*.cshtml'
---

# Razor Pages and Views (CSHTML) Coding Standards

This document outlines the CSHTML coding standards to ensure consistency, maintainability, security, and performance across all Razor views and pages.

## General Principles

Follow these Razor syntax guidelines and ASP.NET Core MVC best practices. Prioritize security, accessibility, performance, and maintainability in all view implementations.

## File Organization and Structure

### File Naming Conventions
- Use PascalCase for view files: `UserDetails.cshtml`, `AdminActions.cshtml`
- Partial views prefix with underscore: `_UserCard.cshtml`, `_NavigationMenu.cshtml`
- Layout files prefix with underscore: `_Layout.cshtml`, `_AdminLayout.cshtml`
- ViewImports and ViewStart: `_ViewImports.cshtml`, `_ViewStart.cshtml`

### Directory Structure
```
Views/
├── Shared/
│   ├── _Layout.cshtml
│   ├── _AdminLayout.cshtml
│   ├── _LoginPartial.cshtml
│   └── Error.cshtml
├── Home/
│   ├── Index.cshtml
│   └── Privacy.cshtml
├── Users/
│   ├── Index.cshtml
│   ├── Details.cshtml
│   └── _UserCard.cshtml
└── _ViewImports.cshtml
```

## Razor Syntax Best Practices

### Code Blocks and Expressions
```html
@* Use explicit code blocks for multi-line C# *@
@{
    var userName = User.Identity.Name;
    var isAdmin = User.IsInRole("Admin");
    var pageTitle = $"Welcome, {userName}";
}

@* Use inline expressions for simple values *@
<h1>@pageTitle</h1>
<p>Current time: @DateTime.Now.ToString("F")</p>

@* Use parentheses for complex expressions *@
<p>@(Model.Users?.Count ?? 0) users found</p>
<span class="@(isAdmin ? "admin-badge" : "user-badge")">@userName</span>
```

### HTML Encoding and Security
```html
@* Always use @ for automatic HTML encoding *@
<p>User input: @Model.UserInput</p>

@* Use Html.Raw only for trusted content *@
<div class="content">
    @Html.Raw(Model.TrustedHtmlContent)
</div>

@* Use explicit encoding for attributes *@
<input type="text" value="@Model.SearchTerm" />
<a href="@Url.Action("Details", new { id = Model.Id })">View Details</a>

@* Never use raw string interpolation for user content *@
@* WRONG: <p>@($"Hello {Model.UserName}")</p> *@
@* CORRECT: <p>Hello @Model.UserName</p> *@
```

## Model Binding and ViewModels

### Strongly Typed Views
```html
@* Always use strongly typed models *@
@model UserDetailsViewModel

<h1>@Model.FullName</h1>
<p>Email: @Model.Email</p>
<p>Registration Date: @Model.RegistrationDate.ToString("MMMM dd, yyyy")</p>

@* Use ViewBag/ViewData sparingly and with constants *@
@{
    ViewData["Title"] = "User Details";
    ViewData["ActivePage"] = "Users";
}
```

### Form Binding and Validation
```html
@model CreateUserViewModel

<form asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    
    <div class="form-group">
        <label asp-for="Email" class="form-label"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Password" class="form-label"></label>
        <input asp-for="Password" type="password" class="form-control" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Create User</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

## Tag Helpers and HTML Helpers

### Prefer Tag Helpers Over HTML Helpers
```html
@* Use Tag Helpers (preferred) *@
<a asp-controller="Users" asp-action="Details" asp-route-id="@Model.Id" class="btn btn-primary">
    View Details
</a>

<form asp-controller="Users" asp-action="Update" asp-route-id="@Model.Id" method="post">
    <input asp-for="Name" class="form-control" />
    <button type="submit" class="btn btn-success">Save</button>
</form>

@* HTML Helpers (legacy, avoid in new code) *@
@* @Html.ActionLink("View Details", "Details", "Users", new { id = Model.Id }, new { @class = "btn btn-primary" }) *@
```

### Custom Tag Helpers Usage
```html
@* Use custom tag helpers for reusable components *@
<user-avatar user-id="@Model.Id" size="large" show-status="true"></user-avatar>

<game-server-status server-id="@Model.ServerId" refresh-interval="30"></game-server-status>

<admin-action-badge action-type="@Model.ActionType" severity="@Model.Severity"></admin-action-badge>
```

## Layout and Partial Views

### Layout Structure
```html
@* _Layout.cshtml *@
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - XtremeIdiots Portal</title>
    
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    
    @await RenderSectionAsync("Styles", required: false)
</head>
<body>
    <header>
        <partial name="_NavigationPartial" />
    </header>
    
    <main class="container">
        @RenderBody()
    </main>
    
    <footer>
        <partial name="_FooterPartial" />
    </footer>
    
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Partial Views
```html
@* _UserCard.cshtml *@
@model UserCardViewModel

<div class="user-card" data-user-id="@Model.Id">
    <div class="user-avatar">
        <img src="@Model.AvatarUrl" alt="@Model.DisplayName avatar" class="rounded-circle" />
    </div>
    <div class="user-info">
        <h5 class="user-name">@Model.DisplayName</h5>
        <p class="user-email text-muted">@Model.Email</p>
        <span class="badge @(Model.IsOnline ? "bg-success" : "bg-secondary")">
            @(Model.IsOnline ? "Online" : "Offline")
        </span>
    </div>
    <div class="user-actions">
        <a asp-controller="Users" asp-action="Details" asp-route-id="@Model.Id" 
           class="btn btn-sm btn-outline-primary">View Profile</a>
    </div>
</div>
```

## Conditional Rendering and Loops

### Conditional Statements
```html
@* Use if statements for conditional rendering *@
@if (User.Identity.IsAuthenticated)
{
    <div class="user-menu">
        <span>Welcome, @User.Identity.Name!</span>
        <a asp-area="Identity" asp-page="/Account/Logout">Logout</a>
    </div>
}
else
{
    <div class="auth-links">
        <a asp-area="Identity" asp-page="/Account/Login">Login</a>
        <a asp-area="Identity" asp-page="/Account/Register">Register</a>
    </div>
}

@* Use null-conditional operators *@
@if (Model.Users?.Any() == true)
{
    <div class="users-list">
        @* Render users *@
    </div>
}
else
{
    <div class="empty-state">
        <p>No users found.</p>
    </div>
}
```

### Loops and Iterations
```html
@* Use foreach for collections *@
@if (Model.Players?.Any() == true)
{
    <div class="players-grid">
        @foreach (var player in Model.Players)
        {
            <div class="player-card" data-player-id="@player.Id">
                <h6>@player.UserName</h6>
                <p>Level: @player.Level</p>
                <p>Score: @player.Score.ToString("N0")</p>
                
                @if (player.IsOnline)
                {
                    <span class="badge bg-success">Online</span>
                }
                
                <div class="actions">
                    <a asp-controller="Players" asp-action="Details" asp-route-id="@player.Id" 
                       class="btn btn-sm btn-primary">View</a>
                    
                    @if (User.IsInRole("Admin"))
                    {
                        <a asp-controller="Admin" asp-action="ManagePlayer" asp-route-id="@player.Id" 
                           class="btn btn-sm btn-warning">Manage</a>
                    }
                </div>
            </div>
        }
    </div>
}

@* Use for loops when index is needed *@
@for (int i = 0; i < Model.GameServers.Count; i++)
{
    <div class="server-row @(i % 2 == 0 ? "even" : "odd")">
        <span class="server-name">@Model.GameServers[i].Name</span>
        <span class="server-players">@Model.GameServers[i].CurrentPlayers/@Model.GameServers[i].MaxPlayers</span>
    </div>
}
```

## Authorization and Security

### Authorization Checks
```html
@* Use policy-based authorization *@
@if ((await AuthorizationService.AuthorizeAsync(User, AuthPolicies.AccessAdminActions)).Succeeded)
{
    <div class="admin-panel">
        <h4>Admin Actions</h4>
        <a asp-controller="AdminActions" asp-action="Create" class="btn btn-danger">
            Create Admin Action
        </a>
    </div>
}

@* Check for specific roles *@
@if (User.IsInRole("Moderator") || User.IsInRole("Admin"))
{
    <div class="moderator-tools">
        <button type="button" class="btn btn-warning" data-bs-toggle="modal" data-bs-target="#banPlayerModal">
            Ban Player
        </button>
    </div>
}

@* Use authorization tag helper *@
<div>
    <authorize policy="@AuthPolicies.ManageServers">
        <a asp-controller="Servers" asp-action="Create" class="btn btn-primary">Add Server</a>
    </authorize>
</div>
```

### CSRF Protection
```html
@* Always include anti-forgery tokens in forms *@
<form asp-action="Delete" asp-route-id="@Model.Id" method="post" 
      onsubmit="return confirm('Are you sure you want to delete this item?')">
    @Html.AntiForgeryToken()
    <button type="submit" class="btn btn-danger">Delete</button>
</form>

@* Use asp-* attributes which automatically include tokens *@
<form asp-controller="Players" asp-action="Update" asp-route-id="@Model.Id" method="post">
    <input asp-for="UserName" class="form-control" />
    <button type="submit" class="btn btn-success">Save</button>
</form>
```

## Performance Optimization

### Efficient Resource Loading
```html
@* Use asp-append-version for cache busting *@
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
<script src="~/js/site.js" asp-append-version="true"></script>

@* Use CDN with fallback *@
<script src="https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.6.0.min.js"
        asp-fallback-src="~/lib/jquery/dist/jquery.min.js"
        asp-fallback-test="window.jQuery"
        crossorigin="anonymous"
        integrity="sha384-vtXRMe3mGCbOeY7l30aIg8H9p3GdeSe4IFlP6G8JMa7o7lXvnz3GlVBAJ8BKG">
</script>

@* Defer non-critical scripts *@
<script src="~/js/analytics.js" asp-append-version="true" defer></script>
```

### Minimize View Logic
```html
@* Move complex logic to ViewModels or ViewComponents *@
@model DashboardViewModel

@* Good: Use computed properties from ViewModel *@
<div class="stats-summary">
    <div class="stat-card">
        <h3>@Model.TotalPlayers</h3>
        <p>Total Players</p>
    </div>
    <div class="stat-card">
        <h3>@Model.OnlinePlayersCount</h3>
        <p>Online Now</p>
    </div>
    <div class="stat-card">
        <h3>@Model.ActiveServersCount</h3>
        <p>Active Servers</p>
    </div>
</div>

@* Use ViewComponents for complex rendering logic *@
@await Component.InvokeAsync("PlayerStatistics", new { playerId = Model.PlayerId })
@await Component.InvokeAsync("ServerStatusWidget", new { serverId = Model.ServerId })
```

## Accessibility Best Practices

### Semantic HTML and ARIA
```html
@* Use semantic HTML elements *@
<nav aria-label="Main navigation">
    <ul class="nav-menu">
        <li><a href="/" aria-current="@(ViewContext.RouteData.Values["action"]?.ToString() == "Index" ? "page" : null)">Home</a></li>
        <li><a href="/players">Players</a></li>
        <li><a href="/servers">Servers</a></li>
    </ul>
</nav>

@* Proper form labels and descriptions *@
<div class="form-group">
    <label for="search-input" class="form-label">Search Players</label>
    <input id="search-input" type="text" asp-for="SearchTerm" class="form-control" 
           aria-describedby="search-help" placeholder="Enter player name or ID">
    <div id="search-help" class="form-text">Search by username, GUID, or IP address</div>
</div>

@* Use ARIA labels for icons and buttons *@
<button type="button" class="btn btn-primary" aria-label="Refresh server status">
    <i class="fas fa-refresh" aria-hidden="true"></i>
</button>

@* Proper heading hierarchy *@
<main>
    <h1>Player Management</h1>
    <section>
        <h2>Search Results</h2>
        @foreach (var player in Model.Players)
        {
            <article class="player-card">
                <h3>@player.UserName</h3>
                <p>Player details...</p>
            </article>
        }
    </section>
</main>
```

### Focus Management
```html
@* Ensure proper tab order *@
<form>
    <input type="text" asp-for="UserName" tabindex="1" />
    <input type="email" asp-for="Email" tabindex="2" />
    <input type="password" asp-for="Password" tabindex="3" />
    <button type="submit" tabindex="4">Submit</button>
    <a href="/cancel" tabindex="5">Cancel</a>
</form>

@* Skip links for accessibility *@
<a href="#main-content" class="skip-link">Skip to main content</a>
<main id="main-content">
    @RenderBody()
</main>
```

## Error Handling and User Feedback

### Error Display Patterns
```html
@* Global error summary *@
@if (ViewData.ModelState.ErrorCount > 0)
{
    <div class="alert alert-danger" role="alert">
        <h4 class="alert-heading">Please correct the following errors:</h4>
        <div asp-validation-summary="All"></div>
    </div>
}

@* Field-specific validation *@
<div class="form-group">
    <label asp-for="Email" class="form-label"></label>
    <input asp-for="Email" class="form-control @(Html.ViewData.ModelState.IsValidField("Email") ? "" : "is-invalid")" />
    <span asp-validation-for="Email" class="invalid-feedback"></span>
</div>

@* Success messages *@
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
```

### Loading States and Progressive Enhancement
```html
@* Loading indicators *@
<div id="loading-indicator" class="d-none">
    <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
</div>

@* Progressive enhancement with fallbacks *@
<div class="data-table" data-ajax-url="@Url.Action("GetPlayersData")">
    @* Server-rendered fallback content *@
    <table class="table">
        <tbody>
            @foreach (var player in Model.Players)
            {
                <tr>
                    <td>@player.UserName</td>
                    <td>@player.Level</td>
                    <td>@player.LastSeen.ToString("yyyy-MM-dd")</td>
                </tr>
            }
        </tbody>
    </table>
    
    @* Enhanced with JavaScript for better UX *@
    <noscript>
        <p>JavaScript is required for enhanced functionality.</p>
    </noscript>
</div>
```

## Sections and Content Organization

### Script and Style Sections
```html
@* Define sections in layout *@
@section Styles {
    <link rel="stylesheet" href="~/css/players.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/css/datatables.css" asp-append-version="true" />
}

@section Scripts {
    <script src="~/lib/datatables/js/jquery.dataTables.min.js"></script>
    <script src="~/js/players.js" asp-append-version="true"></script>
    
    <script>
        $(document).ready(function() {
            $('#players-table').DataTable({
                responsive: true,
                pageLength: 25,
                order: [[3, 'desc']] // Sort by last seen
            });
        });
    </script>
}

@* Content sections for flexible layouts *@
@section Breadcrumb {
    <nav aria-label="breadcrumb">
        <ol class="breadcrumb">
            <li class="breadcrumb-item"><a href="/">Home</a></li>
            <li class="breadcrumb-item"><a href="/players">Players</a></li>
            <li class="breadcrumb-item active" aria-current="page">@Model.UserName</li>
        </ol>
    </nav>
}
```

## Common Anti-Patterns to Avoid

### Security Anti-Patterns
```html
@* NEVER do this - XSS vulnerability *@
@* <div>@Html.Raw(Model.UserGeneratedContent)</div> *@

@* NEVER do this - Direct HTML concatenation *@
@* <script>var userName = "@Model.UserName";</script> *@

@* NEVER do this - Unsanitized URLs *@
@* <a href="@Model.RedirectUrl">Click here</a> *@

@* DO this instead *@
<div>@Model.UserGeneratedContent</div>
<script>var userName = @Json.Serialize(Model.UserName);</script>
<a href="@Url.IsLocalUrl(Model.RedirectUrl) ? Model.RedirectUrl : "/"">Click here</a>
```

### Performance Anti-Patterns
```html
@* AVOID - Database queries in views *@
@* @foreach (var user in DbContext.Users.ToList()) *@

@* AVOID - Complex business logic in views *@
@* @{ var complexCalculation = Model.Orders.Where(o => o.Status == "Pending").Sum(o => o.Total * 0.1); } *@

@* AVOID - Synchronous calls in async contexts *@
@* @{ var result = SomeService.GetDataAsync().Result; } *@

@* DO this instead - Use ViewModels with pre-computed data *@
@foreach (var user in Model.Users)
{
    <div>@user.DisplayName</div>
}
<div class="total">Total: @Model.PrecomputedTotal</div>
```

### Maintainability Anti-Patterns
```html
@* AVOID - Inline styles and scripts *@
@* <div style="color: red; font-size: 14px;">Error</div> *@
@* <script>function doSomething() { alert('test'); }</script> *@

@* AVOID - Magic numbers and strings *@
@* @if (Model.Status == 1) *@
@* <div class="col-3">...</div> *@

@* DO this instead - Use CSS classes and external scripts *@
<div class="error-message">Error</div>
@if (Model.Status == PlayerStatus.Active)
{
    <div class="@BootstrapClasses.GridColumn.Small3">...</div>
}
```

## Code Formatting and Style

### Indentation and Spacing
```html
@model UserViewModel

@{
    ViewData["Title"] = "User Profile";
    var isCurrentUser = Model.Id == User.GetUserId();
}

<div class="user-profile">
    <div class="profile-header">
        <img src="@Model.AvatarUrl" 
             alt="@Model.DisplayName avatar" 
             class="profile-avatar" />
        
        <div class="profile-info">
            <h1>@Model.DisplayName</h1>
            <p class="text-muted">@Model.Email</p>
            
            @if (isCurrentUser)
            {
                <a asp-action="Edit" 
                   asp-route-id="@Model.Id" 
                   class="btn btn-primary">
                    Edit Profile
                </a>
            }
        </div>
    </div>
    
    <div class="profile-content">
        @await Html.PartialAsync("_UserStats", Model.Statistics)
        @await Html.PartialAsync("_UserActivity", Model.RecentActivity)
    </div>
</div>

@section Scripts {
    <script src="~/js/user-profile.js" asp-append-version="true"></script>
}
```

### Comments and Documentation
```html
@*
    Player Details View
    Displays comprehensive player information including:
    - Basic profile information
    - Game statistics
    - Admin actions history
    - Current ban status
*@
@model PlayerDetailsViewModel

@{
    ViewData["Title"] = $"Player: {Model.UserName}";
    ViewData["Description"] = $"View detailed information for player {Model.UserName}";
}

@* Main player information card *@
<div class="player-profile-card">
    @* ... content ... *@
</div>

@* Admin-only section for player management *@
@if (User.IsInRole("Admin"))
{
    @* Admin tools and actions for player management *@
    <div class="admin-actions">
        @* ... admin content ... *@
    </div>
}
```

This comprehensive guide ensures that all CSHTML files in the XtremeIdiots Portal follow Azure/Microsoft best practices for security, performance, accessibility, and maintainability.
