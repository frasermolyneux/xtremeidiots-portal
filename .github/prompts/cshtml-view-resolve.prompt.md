---
mode: agent
---

# CSHTML View Resolution Prompt

Use this prompt to automatically resolve and implement recommendations from CSHTML view reviews based on the project's coding standards.

## Instructions

This prompt should be used **after** completing a CSHTML view review. It will help you systematically resolve the identified issues and implement best practices for Razor views and pages.

## Resolution Framework

### Input Requirements
Before using this prompt, you should have:
1. **Original CSHTML view code**
2. **Review feedback** with specific issues identified
3. **Priority list** of changes to implement
4. **View context** (controller, ViewModel, layout dependencies)

### Resolution Process

#### Phase 1: Critical Fixes (Security & Accessibility)
Execute these changes first as they impact security and user experience:

1. **Security Issues**
   - Fix XSS vulnerabilities (remove unsafe `Html.Raw` usage)
   - Add anti-forgery token protection to forms
   - Implement proper URL validation for redirects
   - Add authorization checks for sensitive operations

2. **Accessibility Issues**
   - Add missing alt text to images
   - Associate labels with form inputs
   - Implement proper heading hierarchy
   - Add ARIA attributes to interactive elements

#### Phase 2: Code Standards Compliance
Implement these changes to align with CSHTML coding standards:

1. **Razor Syntax**
   - Fix improper code block usage
   - Correct HTML encoding patterns
   - Implement proper null-conditional operators
   - Replace legacy HTML helpers with Tag helpers

2. **Structure and Organization**
   - Organize sections properly (Styles, Scripts)
   - Implement strongly typed models
   - Add proper ViewData/metadata
   - Correct file naming and organization

#### Phase 3: Performance and User Experience
Apply these optimizations and improvements:

1. **Performance Enhancements**
   - Add cache busting to static resources
   - Implement proper script loading (defer, async)
   - Move complex logic to ViewModels
   - Optimize resource loading patterns

2. **User Experience**
   - Add loading indicators and feedback
   - Implement proper error handling displays
   - Enhance form validation presentation
   - Improve responsive design patterns

## Resolution Templates

### Template 1: Fixing Security Vulnerabilities

```html
<!-- BEFORE (Vulnerable to XSS) -->
@model UserProfileViewModel
<div class="user-bio">
    @Html.Raw(Model.Biography)
</div>
<script>
    var userName = "@Model.UserName";
</script>
<a href="@Model.RedirectUrl">Continue</a>

<!-- AFTER (Secure) -->
@model UserProfileViewModel
<div class="user-bio">
    @Model.Biography
</div>
<script>
    var userName = @Json.Serialize(Model.UserName);
</script>
<a href="@(Url.IsLocalUrl(Model.RedirectUrl) ? Model.RedirectUrl : "/")">Continue</a>
```

### Template 2: Implementing Proper Form Security and Validation

```html
<!-- BEFORE (Insecure and poor validation) -->
<form action="/Users/Update" method="post">
    <input type="text" name="UserName" value="@Model.UserName" />
    <input type="email" name="Email" value="@Model.Email" />
    <button type="submit">Save</button>
</form>

<!-- AFTER (Secure with proper validation) -->
@model UpdateUserViewModel

<form asp-controller="Users" asp-action="Update" asp-route-id="@Model.Id" method="post">
    <div asp-validation-summary="ModelOnly" class="alert alert-danger" role="alert"></div>
    
    <div class="form-group">
        <label asp-for="UserName" class="form-label">Username</label>
        <input asp-for="UserName" class="form-control" />
        <span asp-validation-for="UserName" class="invalid-feedback"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Email" class="form-label">Email Address</label>
        <input asp-for="Email" type="email" class="form-control" />
        <span asp-validation-for="Email" class="invalid-feedback"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Save Changes</button>
    <a asp-controller="Users" asp-action="Details" asp-route-id="@Model.Id" 
       class="btn btn-secondary">Cancel</a>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### Template 3: Adding Accessibility Features

```html
<!-- BEFORE (Poor accessibility) -->
<div class="player-card">
    <img src="@Model.AvatarUrl" />
    <div>@Model.PlayerName</div>
    <button onclick="banPlayer()">Ban</button>
</div>

<form>
    <input type="text" placeholder="Search players" />
    <button>Search</button>
</form>

<!-- AFTER (Accessible) -->
<article class="player-card" role="article">
    <img src="@Model.AvatarUrl" 
         alt="@Model.PlayerName avatar" 
         class="player-avatar" />
    
    <div class="player-info">
        <h3 class="player-name">@Model.PlayerName</h3>
        <p class="player-status">
            <span class="visually-hidden">Player status:</span>
            @(Model.IsOnline ? "Online" : "Offline")
        </p>
    </div>
    
    @if (User.IsInRole("Admin"))
    {
        <div class="player-actions">
            <button type="button" 
                    class="btn btn-warning" 
                    data-bs-toggle="modal" 
                    data-bs-target="#banPlayerModal"
                    data-player-id="@Model.Id"
                    aria-label="Ban player @Model.PlayerName">
                <i class="fas fa-ban" aria-hidden="true"></i>
                Ban Player
            </button>
        </div>
    }
</article>

<form asp-controller="Players" asp-action="Search" method="get" role="search">
    <div class="form-group">
        <label for="search-input" class="form-label">Search Players</label>
        <input id="search-input" 
               asp-for="SearchTerm" 
               type="search" 
               class="form-control" 
               placeholder="Enter player name, GUID, or IP address"
               aria-describedby="search-help" />
        <div id="search-help" class="form-text">
            Search by username, GUID, or IP address
        </div>
    </div>
    <button type="submit" class="btn btn-primary">
        <i class="fas fa-search" aria-hidden="true"></i>
        Search
    </button>
</form>
```

### Template 4: Replacing Legacy HTML Helpers with Tag Helpers

```html
<!-- BEFORE (Legacy HTML Helpers) -->
@Html.ActionLink("View Details", "Details", "Players", new { id = Model.Id }, new { @class = "btn btn-primary" })

@using (Html.BeginForm("Update", "Players", new { id = Model.Id }, FormMethod.Post))
{
    @Html.LabelFor(m => m.UserName)
    @Html.TextBoxFor(m => m.UserName, new { @class = "form-control" })
    @Html.ValidationMessageFor(m => m.UserName)
    
    <button type="submit">Save</button>
}

<!-- AFTER (Modern Tag Helpers) -->
<a asp-controller="Players" 
   asp-action="Details" 
   asp-route-id="@Model.Id" 
   class="btn btn-primary">
    View Details
</a>

<form asp-controller="Players" asp-action="Update" asp-route-id="@Model.Id" method="post">
    <div class="form-group">
        <label asp-for="UserName" class="form-label"></label>
        <input asp-for="UserName" class="form-control" />
        <span asp-validation-for="UserName" class="invalid-feedback"></span>
    </div>
    
    <button type="submit" class="btn btn-success">Save Changes</button>
</form>
```

### Template 5: Implementing Proper Authorization Checks

```html
<!-- BEFORE (No authorization checks) -->
<div class="admin-panel">
    <h4>Admin Actions</h4>
    <a href="/AdminActions/Create">Create Admin Action</a>
    <a href="/Players/BanAll">Ban All Players</a>
</div>

<!-- AFTER (Proper authorization) -->
@inject IAuthorizationService AuthorizationService

@if ((await AuthorizationService.AuthorizeAsync(User, AuthPolicies.AccessAdminActions)).Succeeded)
{
    <div class="admin-panel">
        <h4>Admin Actions</h4>
        
        @if ((await AuthorizationService.AuthorizeAsync(User, AuthPolicies.CreateAdminAction)).Succeeded)
        {
            <a asp-controller="AdminActions" 
               asp-action="Create" 
               class="btn btn-primary">
                Create Admin Action
            </a>
        }
        
        @if (User.IsInRole("SuperAdmin"))
        {
            <button type="button" 
                    class="btn btn-danger" 
                    data-bs-toggle="modal" 
                    data-bs-target="#massActionModal"
                    data-action="ban-all">
                <i class="fas fa-exclamation-triangle" aria-hidden="true"></i>
                Mass Ban Players
            </button>
        }
    </div>
}
else
{
    <div class="alert alert-info" role="alert">
        <i class="fas fa-info-circle" aria-hidden="true"></i>
        You don't have permission to access admin functions.
    </div>
}
```

### Template 6: Adding Performance Optimizations

```html
<!-- BEFORE (Poor performance patterns) -->
<head>
    <link rel="stylesheet" href="/css/site.css" />
    <script src="/js/jquery.js"></script>
    <script src="/js/bootstrap.js"></script>
    <script src="/js/site.js"></script>
</head>

<body>
    @foreach (var player in DbContext.Players.ToList())
    {
        <div>@player.UserName - @CalculateComplexStats(player)</div>
    }
</body>

<!-- AFTER (Optimized) -->
<head>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    
    @await RenderSectionAsync("Styles", required: false)
</head>

<body>
    @* Use pre-computed data from ViewModel *@
    @if (Model.Players?.Any() == true)
    {
        <div class="players-grid">
            @foreach (var player in Model.Players)
            {
                <div class="player-card">
                    <h6>@player.UserName</h6>
                    <p>Statistics: @player.PrecomputedStats</p>
                </div>
            }
        </div>
    }
    else
    {
        <div class="empty-state">
            <p>No players found.</p>
        </div>
    }
    
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true" defer></script>
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
```

### Template 7: Implementing Proper Error Handling and User Feedback

```html
<!-- BEFORE (Poor error handling) -->
@if (ViewBag.Error != null)
{
    <div style="color: red;">@ViewBag.Error</div>
}

<form method="post">
    <input name="UserName" />
    <button type="submit">Save</button>
</form>

<!-- AFTER (Comprehensive error handling) -->
@model PlayerUpdateViewModel

@* Global error summary *@
@if (ViewData.ModelState.ErrorCount > 0)
{
    <div class="alert alert-danger" role="alert">
        <h4 class="alert-heading">
            <i class="fas fa-exclamation-triangle" aria-hidden="true"></i>
            Please correct the following errors:
        </h4>
        <div asp-validation-summary="All"></div>
    </div>
}

@* Success message *@
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        <i class="fas fa-check-circle" aria-hidden="true"></i>
        @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}

@* Form with proper validation feedback *@
<form asp-controller="Players" asp-action="Update" asp-route-id="@Model.Id" method="post">
    <div class="form-group">
        <label asp-for="UserName" class="form-label">Username</label>
        <input asp-for="UserName" 
               class="form-control @(Html.ViewData.ModelState.IsValidField("UserName") ? "" : "is-invalid")" 
               aria-describedby="username-help" />
        <div id="username-help" class="form-text">
            Choose a unique username for this player
        </div>
        <span asp-validation-for="UserName" class="invalid-feedback"></span>
    </div>
    
    <div class="form-actions">
        <button type="submit" class="btn btn-primary">
            <span class="spinner-border spinner-border-sm d-none" role="status" aria-hidden="true"></span>
            Save Changes
        </button>
        <a asp-controller="Players" asp-action="Details" asp-route-id="@Model.Id" 
           class="btn btn-secondary">Cancel</a>
    </div>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        // Add loading state to form submission
        document.querySelector('form').addEventListener('submit', function(e) {
            const submitBtn = e.target.querySelector('button[type="submit"]');
            const spinner = submitBtn.querySelector('.spinner-border');
            submitBtn.disabled = true;
            spinner.classList.remove('d-none');
        });
    </script>
}
```

### Template 8: Gaming Portal Specific Improvements

```html
<!-- BEFORE (Basic game server display) -->
<div>
    Server: @Model.ServerName - @Model.PlayerCount players
    <a href="/servers/@Model.Id">View</a>
</div>

<!-- AFTER (Enhanced gaming portal display) -->
<div class="game-server-card" data-server-id="@Model.Id">
    <div class="server-header">
        <div class="server-info">
            <h5 class="server-name">
                <i class="fas fa-server" aria-hidden="true"></i>
                @Model.ServerName
            </h5>
            <span class="badge @(Model.IsOnline ? "bg-success" : "bg-danger")">
                @(Model.IsOnline ? "Online" : "Offline")
            </span>
        </div>
        
        <div class="server-game-type">
            <img src="~/images/games/@(Model.GameType.ToString().ToLower()).png" 
                 alt="@Model.GameType game icon" 
                 class="game-icon" />
            <span class="game-name">@Model.GameType.GetDisplayName()</span>
        </div>
    </div>
    
    <div class="server-stats">
        <div class="stat-item">
            <span class="stat-label">Players:</span>
            <span class="stat-value">
                @Model.CurrentPlayers/@Model.MaxPlayers
                <div class="progress" style="width: 100px; height: 8px;">
                    <div class="progress-bar" 
                         role="progressbar" 
                         style="width: @((Model.CurrentPlayers / (double)Model.MaxPlayers) * 100)%"
                         aria-valuenow="@Model.CurrentPlayers" 
                         aria-valuemin="0" 
                         aria-valuemax="@Model.MaxPlayers"
                         aria-label="Server capacity: @Model.CurrentPlayers out of @Model.MaxPlayers players">
                    </div>
                </div>
            </span>
        </div>
        
        <div class="stat-item">
            <span class="stat-label">Map:</span>
            <span class="stat-value">@Model.CurrentMap</span>
        </div>
        
        @if (Model.IsOnline)
        {
            <div class="stat-item">
                <span class="stat-label">Uptime:</span>
                <span class="stat-value">@Model.Uptime.ToString(@"dd\.hh\:mm")</span>
            </div>
        }
    </div>
    
    <div class="server-actions">
        <a asp-controller="Servers" 
           asp-action="Details" 
           asp-route-id="@Model.Id" 
           class="btn btn-sm btn-primary">
            <i class="fas fa-info-circle" aria-hidden="true"></i>
            View Details
        </a>
        
        @if (User.IsInRole("Admin"))
        {
            <button type="button" 
                    class="btn btn-sm btn-warning" 
                    data-bs-toggle="modal" 
                    data-bs-target="#serverManagementModal"
                    data-server-id="@Model.Id"
                    aria-label="Manage server @Model.ServerName">
                <i class="fas fa-cog" aria-hidden="true"></i>
                Manage
            </button>
        }
    </div>
</div>
```

## Resolution Checklist

### ✅ Critical Fixes Applied
- [ ] Fixed XSS vulnerabilities (removed unsafe `Html.Raw` usage)
- [ ] Added anti-forgery token protection to all forms
- [ ] Implemented proper URL validation for redirects
- [ ] Added authorization checks for sensitive operations
- [ ] Fixed accessibility issues (alt text, labels, ARIA attributes)
- [ ] Implemented proper heading hierarchy
- [ ] Added keyboard navigation support

### ✅ Code Standards Applied
- [ ] Replaced legacy HTML helpers with Tag helpers
- [ ] Implemented strongly typed models with `@model` directive
- [ ] Used proper Razor syntax for code blocks and expressions
- [ ] Added appropriate ViewData/metadata
- [ ] Organized sections properly (Styles, Scripts)
- [ ] Added comprehensive validation display
- [ ] Used null-conditional operators appropriately

### ✅ Performance & UX Applied
- [ ] Added cache busting with `asp-append-version`
- [ ] Implemented proper script loading (defer, async)
- [ ] Moved complex logic to ViewModels/ViewComponents
- [ ] Added loading indicators for async operations
- [ ] Implemented proper error handling displays
- [ ] Enhanced responsive design patterns
- [ ] Optimized resource loading order

### ✅ Gaming Portal Specific
- [ ] Enhanced game server displays with proper styling
- [ ] Added player management interface improvements
- [ ] Implemented admin action authorization checks
- [ ] Added game-specific visual elements (icons, badges)
- [ ] Enhanced player statistics displays
- [ ] Improved real-time data presentation

## Step-by-Step Resolution Process

### 1. Analyze Review Feedback
```
Input: CSHTML review feedback with categorized issues
Output: Prioritized list of view changes needed
```

### 2. Apply Security Fixes First
```
Focus: XSS prevention, CSRF protection, authorization
Priority: Critical - must be fixed immediately
```

### 3. Implement Accessibility Improvements
```
Focus: ARIA attributes, semantic HTML, keyboard navigation
Priority: High - impacts user experience and compliance
```

### 4. Apply Code Standards
```
Focus: Tag helpers, proper syntax, validation patterns
Priority: Medium - improves maintainability
```

### 5. Add Performance Optimizations
```
Focus: Resource loading, ViewModels, caching
Priority: Lower - enhances performance
```

### 6. Validate Changes
```
Action: Test the view rendering and functionality
Check: All form submissions work correctly
Verify: Accessibility compliance achieved
Validate: Performance improvements implemented
```

## Usage Instructions

1. **Start with review feedback**: Use the output from the CSHTML view review prompt
2. **Prioritize security and accessibility**: Fix these issues first
3. **Apply resolution templates**: Use the templates as guides for common fixes
4. **Test incrementally**: Validate each major change in the browser
5. **Check responsiveness**: Ensure mobile and desktop compatibility
6. **Validate accessibility**: Test with screen readers and keyboard navigation

## Expected Outcomes

After applying this resolution prompt, your CSHTML view should:
- ✅ **Be secure against XSS and CSRF attacks**
- ✅ **Meet accessibility standards (WCAG guidelines)**
- ✅ **Use modern Tag Helper syntax**
- ✅ **Have proper form validation and error handling**
- ✅ **Include comprehensive user feedback mechanisms**
- ✅ **Demonstrate optimal performance patterns**
- ✅ **Follow gaming portal UI/UX best practices**
- ✅ **Be mobile-responsive and user-friendly**

Remember: Always test your changes in multiple browsers and with different user roles to ensure the view works correctly for all users and scenarios.
