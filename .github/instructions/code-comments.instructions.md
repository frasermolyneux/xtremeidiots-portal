---
description: 'Guidelines for GitHub Copilot to write comments to achieve self-explanatory code with less comments. Examples are in JavaScript but it should work on any language that has comments.'
applyTo: '**'
---

# Self-explanatory Code Commenting Instructions

## Core Principle
**Write code that speaks for itself. Comment only when necessary to explain WHY, not WHAT.**
We do not need inline comments most of the time.

## Comment Types

### 🔧 API Documentation (REQUIRED)
**XML documentation comments for public APIs are MANDATORY and different from inline comments:**

```csharp
/// <summary>
/// Creates a new admin action for the specified player
/// </summary>
/// <param name="model">The create admin action view model containing form data</param>
/// <param name="cancellationToken">Cancellation token for the async operation</param>
/// <returns>Redirects to player details on success, returns view with validation errors on failure</returns>
/// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission to create admin actions</exception>
[HttpPost]
public async Task<IActionResult> Create(CreateAdminActionViewModel model, CancellationToken cancellationToken = default)
```

**API documentation should include:**
- Clear `<summary>` describing what the method does
- `<param>` for each parameter explaining its purpose
- `<returns>` describing the return value and conditions
- `<exception>` for expected exceptions
- `<example>` and `<code>` when helpful for complex APIs

### 🚫 Inline Comments (AVOID UNLESS NECESSARY)

## Commenting Guidelines for Inline Comments

### ❌ AVOID These Inline Comment Types

**Obvious Comments**
```csharp
// Bad: States the obvious
var counter = 0;  // Initialize counter to zero
counter++;  // Increment counter by one
```

**Redundant Comments**
```csharp
// Bad: Comment repeats the code
private string GetUserName() {
    return user.Name;  // Return the user's name
}
```

### ✅ WRITE These Inline Comment Types

**Complex Business Logic**
```csharp
// Good: Explains WHY this specific calculation
// Apply progressive tax brackets: 10% up to 10k, 20% above
var tax = CalculateProgressiveTax(income, [0.10, 0.20], [10000]);
```

**Non-obvious Algorithms**
```csharp
// Good: Explains the algorithm choice
// Using Floyd-Warshall for all-pairs shortest paths
// because we need distances between all nodes
for (int k = 0; k < vertices; k++) {
    // ... implementation
}
```

**Business Rules and Constraints**
```csharp
// Good: Explains external constraint
// GitHub API rate limit: 5000 requests/hour for authenticated users
await rateLimiter.Wait();
var response = await fetch(githubApiUrl);
```

## Decision Framework

Before writing an inline comment, ask:
1. **Is this API documentation?** → Use XML docs (required for public APIs)
2. **Is the code self-explanatory?** → No inline comment needed
3. **Would a better variable/function name eliminate the need?** → Refactor instead
4. **Does this explain WHY, not WHAT?** → Good inline comment
5. **Will this help future maintainers understand business logic?** → Good inline comment

## Summary

- **API Documentation (XML docs)**: **REQUIRED** for all public methods, properties, and classes
- **Inline Comments**: **AVOID** unless explaining complex business logic, algorithms, or non-obvious constraints
- **The best inline comment is the one you don't need to write because the code is self-documenting**