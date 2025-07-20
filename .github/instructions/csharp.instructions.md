---
description: 'Guidelines for building C# applications'
applyTo: '**/*.cs'
---

# C# Development

> **IMPORTANT**: All formatting and style decisions must follow the project's `.editorconfig` file as the authoritative source of truth.

## C# Instructions
- Always use the latest C# language features available (C# 12+ features like primary constructors, collection expressions).
- Follow the project's editorconfig settings as the source of truth for all formatting and style decisions.
- Write XML documentation comments for public APIs only. Avoid inline comments unless explaining complex business logic.

## General Instructions
- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in XML documentation.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).

## Formatting and Style (Per EditorConfig)

- **Namespaces**: Use file-scoped namespace declarations (`namespace MyNamespace;`)
- **Braces**: Omit braces for single-statement blocks when safe (`csharp_prefer_braces = false`)
- **var Usage**: Use `var` everywhere - for built-in types, when type is apparent, and elsewhere
- **Expression Bodies**: 
  - Use for properties, indexers, and accessors
  - Do NOT use for methods, constructors, or operators
- **Top-level Statements**: Prefer for Program.cs files
- **Primary Constructors**: Use for C# 12+ when appropriate
- **Collection Expressions**: Use when types match exactly (`[]` instead of `new List<T>()`)
- **Null Checks**: Always use `is null` and `is not null` instead of `== null` or `!= null`
- **Pattern Matching**: Prefer switch expressions and pattern matching over traditional constructs
- **Using Directives**: Place outside namespace, don't sort System directives first

## Documentation Standards

- **XML Documentation**: REQUIRED for all public APIs (methods, properties, classes)
  - Include `<summary>`, `<param>`, `<returns>`, `<exception>` tags
  - Add `<example>` and `<code>` for complex APIs
- **Inline Comments**: AVOID unless explaining WHY (business logic, algorithms, constraints)
- **Self-Documenting Code**: Prefer clear variable/method names over explanatory comments

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 9 including environment-specific settings.

## Nullable Reference Types

- Enable and enforce nullable reference types throughout the codebase.
- Declare variables non-nullable by default, check for `null` only at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations - don't add redundant null checks when the type system guarantees non-null.
- Use nullable annotations (`?`) explicitly when null values are expected.

## Modern C# Features (Per EditorConfig)

- **Pattern Matching**: Use `is` patterns, switch expressions, and extended property patterns
- **Index/Range Operators**: Use `^` and `..` operators when appropriate
- **Target-Typed New**: Use `new()` when type is apparent from context
- **Record Types**: Use for immutable data structures
- **Init-Only Properties**: Use `init` accessors for immutable properties after construction
- **Raw String Literals**: Use `"""` for multi-line strings with embedded quotes
- **Collection Expressions**: Use `[]` syntax when types exactly match

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.

## Validation and Error Handling

- Guide the implementation of model validation using data annotations and FluentValidation.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 7807) implementation for standardized error responses.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.
- Explain test-driven development principles as applied to API development.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.

## Deployment and DevOps

- Guide users through containerizing their API using .NET's built-in container support (`dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`).
- Explain the differences between manual Dockerfile creation and .NET's container publishing features.
- Explain CI/CD pipelines for NET applications.
- Demonstrate deployment to Azure App Service, Azure Container Apps, or other hosting options.
- Show how to implement health checks and readiness probes.
- Explain environment-specific configurations for different deployment stages.