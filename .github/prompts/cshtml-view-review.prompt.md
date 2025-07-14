---
mode: ask
---

# CSHTML View Review Prompt

Use this prompt to review CSHTML views and Razor pages against the project's coding standards and best practices.

## Instructions

When reviewing a CSHTML view, analyze it against the following criteria from the CSHTML coding standards. Provide specific feedback for each area that needs improvement.

## Review Checklist

### 1. File Organization and Structure
- [ ] **File naming**: Uses PascalCase for views (e.g., `UserDetails.cshtml`, `AdminActions.cshtml`)
- [ ] **Partial views**: Prefixed with underscore (e.g., `_UserCard.cshtml`, `_NavigationMenu.cshtml`)
- [ ] **Layout files**: Prefixed with underscore (e.g., `_Layout.cshtml`, `_AdminLayout.cshtml`)
- [ ] **Directory structure**: Follows convention with controller-specific folders
- [ ] **ViewImports/ViewStart**: Properly configured in appropriate locations

### 2. Razor Syntax Best Practices
- [ ] **Code blocks**: Uses explicit `@{ }` blocks for multi-line C# code
- [ ] **Inline expressions**: Uses simple `@expression` for single values
- [ ] **Complex expressions**: Uses parentheses `@(expression)` for complex logic
- [ ] **String interpolation**: Avoids raw string interpolation for user content
- [ ] **Null-conditional operators**: Uses `?.` appropriately for safe navigation

### 3. HTML Encoding and Security
- [ ] **Automatic encoding**: Uses `@` for automatic HTML encoding of user input
- [ ] **Html.Raw usage**: Only uses `Html.Raw()` for trusted content
- [ ] **Attribute encoding**: Properly encodes values in HTML attributes
- [ ] **XSS prevention**: No direct HTML concatenation or unsafe user content rendering
- [ ] **URL safety**: Uses `Url.IsLocalUrl()` for redirect validation

### 4. Model Binding and ViewModels
- [ ] **Strongly typed**: Uses `@model` directive with specific ViewModel types
- [ ] **ViewData/ViewBag**: Uses sparingly and with meaningful keys/constants
- [ ] **Model properties**: Accesses model properties safely with null checks
- [ ] **Title and metadata**: Sets appropriate `ViewData["Title"]` and other metadata

### 5. Form Binding and Validation
- [ ] **Tag helpers**: Uses `asp-*` tag helpers for form elements
- [ ] **Validation summary**: Includes `asp-validation-summary` for error display
- [ ] **Field validation**: Uses `asp-validation-for` on form fields
- [ ] **Form security**: Includes anti-forgery token protection
- [ ] **Form structure**: Proper form markup with labels and help text

### 6. Tag Helpers vs HTML Helpers
- [ ] **Modern approach**: Prefers Tag Helpers over legacy HTML Helpers
- [ ] **Navigation**: Uses `asp-controller`, `asp-action`, `asp-route-*` for links
- [ ] **Form posting**: Uses `asp-action`, `asp-controller` for form submissions
- [ ] **Custom tag helpers**: Uses project-specific custom tag helpers appropriately
- [ ] **Legacy avoidance**: Avoids `@Html.ActionLink()` and similar legacy helpers

### 7. Layout and Partial Views
- [ ] **Layout structure**: Proper DOCTYPE, meta tags, and semantic HTML structure
- [ ] **Partial views**: Uses `<partial>` tag helper or `@await Html.PartialAsync()`
- [ ] **ViewComponents**: Uses `@await Component.InvokeAsync()` for complex components
- [ ] **Section management**: Defines and uses sections appropriately (`Scripts`, `Styles`)
- [ ] **RenderBody placement**: Proper main content area structure

### 8. Conditional Rendering and Loops
- [ ] **Conditional logic**: Uses `@if`, `@else` statements appropriately
- [ ] **Null checks**: Includes proper null and empty collection checks
- [ ] **Loop structures**: Uses `@foreach` and `@for` appropriately
- [ ] **Collection safety**: Checks for `?.Any()` before iterating collections
- [ ] **Performance**: Avoids complex logic inside loops

### 9. Authorization and Security
- [ ] **Authorization checks**: Uses proper authorization service calls
- [ ] **Role checks**: Uses `User.IsInRole()` appropriately
- [ ] **Policy authorization**: Uses `AuthorizationService.AuthorizeAsync()` for policies
- [ ] **Authorization tag helper**: Uses `<authorize>` tag helper when appropriate
- [ ] **CSRF protection**: Includes anti-forgery tokens in forms

### 10. Performance Optimization
- [ ] **Resource loading**: Uses `asp-append-version` for cache busting
- [ ] **CDN with fallback**: Implements proper CDN fallback patterns
- [ ] **Script placement**: Defers non-critical scripts appropriately
- [ ] **View logic**: Minimizes complex logic in views (moved to ViewModels)
- [ ] **ViewComponents**: Uses ViewComponents for complex rendering logic

### 11. Accessibility Best Practices
- [ ] **Semantic HTML**: Uses proper HTML5 semantic elements
- [ ] **ARIA attributes**: Includes appropriate ARIA labels and descriptions
- [ ] **Form accessibility**: Proper labels, descriptions, and field associations
- [ ] **Heading hierarchy**: Uses proper h1-h6 heading structure
- [ ] **Focus management**: Implements proper tab order and focus handling
- [ ] **Skip links**: Includes skip navigation links where appropriate

### 12. Error Handling and User Feedback
- [ ] **Error display**: Implements proper error message display patterns
- [ ] **Validation feedback**: Shows field-specific validation errors
- [ ] **Success messages**: Displays success feedback using TempData
- [ ] **Loading states**: Includes loading indicators for async operations
- [ ] **Progressive enhancement**: Provides fallbacks for JavaScript-dependent features

### 13. Sections and Content Organization
- [ ] **Style sections**: Properly defines and uses `@section Styles`
- [ ] **Script sections**: Properly defines and uses `@section Scripts`
- [ ] **Content sections**: Uses additional sections for flexible layouts
- [ ] **Breadcrumbs**: Implements proper breadcrumb navigation where needed
- [ ] **Page structure**: Logical organization of content areas

### 14. Code Formatting and Style
- [ ] **Indentation**: Uses consistent 4-space indentation
- [ ] **Line breaks**: Proper line breaks for readability
- [ ] **Attribute formatting**: Multi-line attributes formatted properly
- [ ] **Comments**: Uses `@*` comments appropriately for documentation
- [ ] **Whitespace**: Consistent spacing around Razor expressions

## Security Anti-Patterns to Check

### ❌ Critical Security Issues:
- `@Html.Raw(Model.UserGeneratedContent)` - XSS vulnerability
- `<script>var userName = "@Model.UserName";</script>` - Direct script injection
- `<a href="@Model.RedirectUrl">` - Unvalidated redirects
- Missing anti-forgery tokens in forms
- No authorization checks for sensitive operations

### ❌ Performance Anti-Patterns:
- Database queries in views: `@foreach (var user in DbContext.Users.ToList())`
- Complex business logic in views
- Synchronous calls in async contexts
- Inline styles and scripts
- No cache busting on static resources

### ❌ Accessibility Anti-Patterns:
- Missing alt text on images
- No labels associated with form inputs
- Poor heading hierarchy
- Missing ARIA attributes on interactive elements
- No keyboard navigation support

### ✅ Preferred Patterns:
- Automatic HTML encoding: `@Model.UserInput`
- JSON serialization: `@Json.Serialize(Model.UserName)`
- URL validation: `@Url.IsLocalUrl(Model.RedirectUrl) ? Model.RedirectUrl : "/"`
- Computed properties from ViewModels
- External CSS and JavaScript files
- Semantic HTML with proper ARIA support

## Review Process

1. **Read the entire view** to understand its purpose and data flow
2. **Check security first** - look for XSS vulnerabilities and missing protections
3. **Verify accessibility** - ensure proper semantic markup and ARIA attributes
4. **Review performance** - check for expensive operations and resource loading
5. **Validate structure** - confirm proper use of layouts, partials, and sections
6. **Check consistency** - ensure adherence to naming and formatting standards

## Example Review Format

```
## CSHTML View Review: [ViewName.cshtml]

### ✅ What's Working Well:
- [List positive aspects found in the view]

### ⚠️ Issues Found:

#### Critical Security Issues:
- [XSS vulnerabilities, missing CSRF protection, etc.]

#### Accessibility Violations:
- [Missing labels, poor semantic structure, etc.]

#### Performance Concerns:
- [Expensive operations, poor resource loading, etc.]

#### Code Standards Violations:
- [Naming conventions, formatting, structure issues]

### 📝 Detailed Feedback:

#### Security and Encoding (Score: X/10)
- [Specific feedback on HTML encoding, XSS prevention, CSRF protection]

#### Accessibility (Score: X/10)
- [Feedback on semantic HTML, ARIA attributes, keyboard navigation]

#### Performance (Score: X/10)
- [Feedback on resource loading, view logic, caching strategies]

#### Tag Helpers and Modern Practices (Score: X/10)
- [Feedback on tag helper usage vs legacy HTML helpers]

#### Form Handling (Score: X/10)
- [Feedback on form structure, validation, binding]

#### Layout and Structure (Score: X/10)
- [Feedback on partial views, sections, content organization]

#### Authorization and Security Checks (Score: X/10)
- [Feedback on authorization implementation, role checks]

#### Code Style and Formatting (Score: X/10)
- [Feedback on indentation, naming, comments, readability]

### 🎯 Priority Actions:
1. [Most critical security and accessibility fixes]
2. [Performance and structure improvements]
3. [Code style and consistency refinements]

### 💡 Suggestions for Enhancement:
- [Recommendations for better user experience]
- [Opportunities for code reuse and componentization]
- [Modern web standards and best practices]
```

## Usage Instructions

To use this prompt effectively:

1. **Paste the CSHTML view code** you want to review
2. **Work through each checklist section** systematically
3. **Prioritize security and accessibility** issues first
4. **Provide specific examples** of problems and solutions
5. **Include code snippets** showing recommended improvements
6. **Focus on the most impactful changes** first

## Common Gaming Portal Specific Patterns

### Player Management Views:
- Check for proper player data encoding
- Verify admin action authorization
- Ensure game server status displays are accessible
- Validate player statistics formatting

### Admin Interface Views:
- Verify elevated permission checks
- Ensure dangerous actions have confirmation dialogs
- Check for proper audit trail integration
- Validate bulk operation safety

### Game Server Views:
- Check real-time data display patterns
- Verify server status accessibility
- Ensure proper error handling for server connectivity
- Validate responsive design for mobile monitoring

### Dashboard and Statistics Views:
- Check for efficient data visualization
- Verify responsive chart and graph implementations
- Ensure proper loading states for async data
- Validate accessibility of data tables and charts

## Example Patterns for XtremeIdiots Portal

### Good Patterns:
```html
@* Proper player card with security and accessibility *@
<div class="player-card" data-player-id="@Model.Id">
    <img src="@Model.AvatarUrl" alt="@Model.DisplayName avatar" class="rounded-circle" />
    <h5>@Model.DisplayName</h5>
    @if (User.IsInRole("Admin"))
    {
        <a asp-controller="Admin" asp-action="ManagePlayer" asp-route-id="@Model.Id" 
           class="btn btn-warning" aria-label="Manage player @Model.DisplayName">
            Manage
        </a>
    }
</div>

@* Proper form with validation and security *@
<form asp-action="BanPlayer" asp-route-id="@Model.Id" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    <div class="form-group">
        <label asp-for="Reason" class="form-label">Ban Reason</label>
        <textarea asp-for="Reason" class="form-control" rows="3" required></textarea>
        <span asp-validation-for="Reason" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-danger" 
            onclick="return confirm('Are you sure you want to ban this player?')">
        Ban Player
    </button>
</form>
```

Remember: The goal is to ensure views are secure, accessible, performant, and maintainable while following the XtremeIdiots Portal coding standards.
