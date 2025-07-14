---
mode: ask
---

# C# Class Review Prompt

Use this prompt to review C# classes against the project's coding standards and best practices.

## Instructions

When reviewing a C# class, analyze it against the following criteria from the C# coding standards. Provide specific feedback for each area that needs improvement.

## Review Checklist

### 1. Naming Conventions
- [ ] **Classes**: Use PascalCase (e.g., `UserManager`, `ApiClientBase`, `ResponseDto`)
- [ ] **Interfaces**: Prefix with "I" using PascalCase (e.g., `IUserService`, `IRepository<T>`)
- [ ] **Methods**: Use PascalCase verbs (e.g., `GetUserAsync`, `CreateClientAsync`)
- [ ] **Properties**: Use PascalCase nouns (e.g., `UserId`, `CreatedDate`, `IsActive`)
- [ ] **Private fields**: Use camelCase (e.g., `httpClient`, `logger`, `configurationOptions`)
- [ ] **Parameters**: Use camelCase (e.g., `userName`, `connectionString`, `retryCount`)
- [ ] **Constants**: Use PascalCase (e.g., `MaxRetryAttempts`, `DefaultTimeoutSeconds`)
- [ ] **Enums and Enum Members**: Use PascalCase (e.g., `UserStatus.Active`)

### 2. File and Namespace Organization
- [ ] **One public type per file**: File should contain only one public class/interface
- [ ] **File name matches type**: File name should match the primary type name
- [ ] **Namespace hierarchy**: Follow project namespace conventions (e.g., `XtremeIdiots.Portal.{Area}.{Feature}`)
- [ ] **Using statements**: Organized with System namespaces first, then third-party, then local project references
- [ ] **Alphabetical ordering**: Using statements should be alphabetically ordered within their groups

### 3. Asynchronous Programming
- [ ] **Async suffix**: All async methods end with "Async"
- [ ] **Return types**: Async methods return `Task` or `Task<T>`
- [ ] **CancellationToken**: Accept `CancellationToken` parameters for async operations
- [ ] **ConfigureAwait**: Use `ConfigureAwait(false)` in library code
- [ ] **Avoid async void**: Only use `async void` for event handlers
- [ ] **Cancellation support**: Check for cancellation in long-running operations

### 4. Error Handling
- [ ] **Custom exceptions**: Domain-specific exceptions inherit from `ApplicationException`
- [ ] **Exception properties**: Include relevant properties (e.g., `UserId` in `UserNotFoundException`)
- [ ] **Logging**: Always log exceptions with appropriate context
- [ ] **Exception handling**: Use try-catch appropriately, don't catch and rethrow without adding value
- [ ] **Stack trace preservation**: Use `throw;` instead of `throw ex;`

### 5. Dependency Injection
- [ ] **Constructor injection**: Use constructor injection for required dependencies
- [ ] **Guard clauses**: Validate null arguments in constructors
- [ ] **Simple constructors**: Constructors should only assign dependencies
- [ ] **Interface dependencies**: Depend on interfaces, not concrete implementations

### 6. HTTP Client Patterns (if applicable)
- [ ] **Typed clients**: Use typed HTTP client pattern
- [ ] **Interface contracts**: Define clear client interfaces
- [ ] **Proper disposal**: Implement proper disposal patterns
- [ ] **Error handling**: Handle HTTP errors appropriately
- [ ] **Logging**: Log HTTP operations with appropriate detail

### 7. Logging
- [ ] **Structured logging**: Use `ILogger<T>` with structured logging
- [ ] **Appropriate levels**: Use correct log levels (Information, Warning, Error)
- [ ] **Context inclusion**: Include relevant context in log messages
- [ ] **No sensitive data**: Avoid logging sensitive information
- [ ] **Performance**: Use log level checks for expensive logging operations

### 8. Performance and Resource Management
- [ ] **Disposal**: Implement `IDisposable` when managing unmanaged resources
- [ ] **Readonly fields**: Use `readonly` for immutable fields
- [ ] **Collection interfaces**: Prefer `IEnumerable<T>` over concrete collections in method signatures
- [ ] **String handling**: Use `StringBuilder` for string concatenation in loops
- [ ] **Memory efficiency**: Consider `Span<T>` and `Memory<T>` for performance-critical code

### 9. Documentation
- [ ] **XML comments**: Use XML documentation for public APIs
- [ ] **Parameter documentation**: Document parameters with `<param>` tags
- [ ] **Return documentation**: Document return values with `<returns>` tags
- [ ] **Exception documentation**: Document exceptions with `<exception>` tags
- [ ] **Summary**: Provide meaningful summaries for methods and classes

### 10. Security
- [ ] **Input validation**: Validate all input parameters
- [ ] **Length checks**: Validate string lengths and ranges
- [ ] **SQL injection**: Use parameterized queries, never string concatenation
- [ ] **Secrets management**: No hardcoded secrets or connection strings
- [ ] **Authorization**: Implement proper authorization checks where needed

### 11. Testing Considerations
- [ ] **Testability**: Classes should be easily testable
- [ ] **Single responsibility**: Classes should have a single, clear responsibility
- [ ] **Dependencies**: Dependencies should be mockable
- [ ] **Public surface**: Minimize public surface area
- [ ] **Static dependencies**: Avoid static dependencies that make testing difficult

### 12. Code Formatting
- [ ] **Indentation**: Use 4 spaces for indentation
- [ ] **Braces**: Place opening braces on new lines for types and methods
- [ ] **Line length**: Keep lines under 120 characters when practical
- [ ] **Blank lines**: Use blank lines to separate logical sections
- [ ] **Consistency**: Follow consistent formatting throughout

## Common Anti-Patterns to Check

### ❌ Avoid These Patterns:
- `async void` methods (except event handlers)
- Blocking async calls with `.Result` or `.Wait()`
- `throw ex;` (loses stack trace)
- String concatenation in loops
- Hardcoded connection strings or secrets
- Catching exceptions without adding value
- Public fields instead of properties
- Null reference without validation

### ✅ Prefer These Patterns:
- `async Task` methods
- `await` with `ConfigureAwait(false)`
- `throw;` (preserves stack trace)
- `StringBuilder` for string building
- Configuration from dependency injection
- Meaningful exception handling with logging
- Properties with appropriate accessibility
- Guard clauses for null validation

## Review Process

1. **Read the entire class** to understand its purpose and responsibilities
2. **Check each section** against the criteria above
3. **Identify violations** and note specific line numbers or code sections
4. **Suggest improvements** with concrete examples
5. **Prioritize feedback** (critical issues vs. style preferences)
6. **Provide positive feedback** for well-implemented patterns

## Example Review Format

```
## C# Class Review: [ClassName]

### ✅ What's Working Well:
- [List positive aspects found in the code]

### ⚠️ Issues Found:

#### Critical Issues:
- [Security, performance, or functionality issues]

#### Code Standards Violations:
- [Naming conventions, formatting, etc.]

#### Suggestions for Improvement:
- [Best practices, refactoring opportunities]

### 📝 Detailed Feedback:

#### Naming Conventions (Score: X/10)
- [Specific feedback with examples]

#### Async Programming (Score: X/10)
- [Specific feedback with examples]

[Continue for each relevant section...]

### 🎯 Priority Actions:
1. [Most important fixes first]
2. [Secondary improvements]
3. [Nice-to-have refinements]
```

## Usage

To use this prompt:
1. Paste the C# class code you want to review
2. Work through each checklist item systematically
3. Provide specific, actionable feedback
4. Include code examples for suggested improvements
5. Focus on the most impactful issues first

Remember: The goal is to help improve code quality while maintaining consistency with project standards.
