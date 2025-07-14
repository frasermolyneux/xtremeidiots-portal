---
applyTo: '*.cs'
---

# C# Coding Standards

This document outlines the C# coding standards to ensure consistency, maintainability, and high-quality code across the project.

## General Principles

Follow these C# coding conventions and the .NET Framework Design Guidelines. Prioritize code readability, maintainability, and performance.

## Naming Conventions

### PascalCase
- **Classes**: `UserManager`, `ApiClientBase`, `ResponseDto`
- **Interfaces**: Prefix with "I" - `IUserService`, `IRepository<T>`, `IAuthenticationProvider`
- **Methods**: `GetUserAsync`, `CreateClientAsync`, `ValidateInput`
- **Properties**: `UserId`, `CreatedDate`, `IsActive`
- **Enums**: `UserStatus`, `LogLevel`, `GameType`
- **Enum Members**: `Active`, `Inactive`, `Pending`
- **Public Fields**: `MaxRetryAttempts`, `DefaultTimeout`
- **Events**: `UserCreated`, `DataProcessed`, `ErrorOccurred`
- **Delegates**: `ProcessDataDelegate`, `ErrorHandlerDelegate`
- **Constants**: `MaxRetryAttempts`, `DefaultTimeoutSeconds`, `ApiVersion`

### camelCase
- **Private fields**: `httpClient`, `logger`, `configurationOptions`
- **Local variables**: `userId`, `requestData`, `cancellationToken`
- **Method parameters**: `userName`, `connectionString`, `retryCount`

### Examples
```csharp
public class UserManager : IUserManager
{
    private readonly ILogger<UserManager> logger;
    private readonly HttpClient httpClient;
    private const int MaxRetryAttempts = 3;

    public string UserName { get; set; }
    public bool IsActive { get; private set; }

    public async Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Implementation
    }

    public event EventHandler<UserEventArgs> UserCreated;
}
```

## File and Namespace Organization

### File Structure
- One public type per file
- File name matches the primary type name
- Use folders to organize related functionality
- Keep using statements at the top, organized alphabetically

### Namespace Conventions
```csharp
// Follow hierarchical structure
namespace XtremeIdiots.Portal.Web.Controllers
namespace XtremeIdiots.Portal.Repository.Abstractions.V2
namespace XtremeIdiots.Portal.Integrations.Forums.Models
```

### Using Statements
```csharp
// System namespaces first
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Third-party packages
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

// Local project references
using XtremeIdiots.Portal.Repository.Abstractions.V2;
using XtremeIdiots.Portal.Web.Models;
```

## Asynchronous Programming

### Async/Await Guidelines
- Suffix all async methods with `Async`
- Always return `Task` or `Task<T>`
- Accept `CancellationToken` parameters for all async operations
- Use `ConfigureAwait(false)` in library code
- Avoid `async void` except for event handlers

```csharp
public async Task<UserDto> GetUserAsync(string userId, CancellationToken cancellationToken = default)
{
    var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
    return mapper.Map<UserDto>(user);
}

public async Task ProcessUsersAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
{
    foreach (var userId in userIds)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ProcessUserAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
```

## Error Handling

### Exception Strategy
- Create domain-specific exceptions inheriting from `ApplicationException` sparingly
- Use built-in exceptions when appropriate
- Always log exceptions with appropriate context
- Implement retry logic for transient failures using Polly

```csharp
public class UserNotFoundException : ApplicationException
{
    public string UserId { get; }

    public UserNotFoundException(string userId) 
        : base($"User with ID '{userId}' was not found")
    {
        UserId = userId;
    }

    public UserNotFoundException(string userId, Exception innerException) 
        : base($"User with ID '{userId}' was not found", innerException)
    {
        UserId = userId;
    }
}

// Usage
public async Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
{
    try
    {
        logger.LogInformation("Retrieving user with ID: {UserId}", userId);
        
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }
        
        return user;
    }
    catch (Exception ex) when (!(ex is UserNotFoundException))
    {
        logger.LogError(ex, "Error retrieving user with ID: {UserId}", userId);
        throw;
    }
}
```

## Dependency Injection and IoC

### Constructor Injection
- Use constructor injection for required dependencies
- Keep constructors simple - only assign dependencies
- Validate null arguments using guard clauses

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private readonly ILogger<UserService> logger;
    private readonly IMapper mapper;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger,
        IMapper mapper)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
}
```

### Service Registration
```csharp
// In Startup.cs or Program.cs
services.AddScoped<IUserService, UserService>();
services.AddSingleton<IConfiguration>(configuration);
services.AddHttpClient<IApiClient, ApiClient>();
```

## HTTP Client Patterns

### Typed HTTP Clients
```csharp
public interface IUserApiClient
{
    Task<UserDto> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
}

public class UserApiClient : IUserApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<UserApiClient> logger;

    public UserApiClient(HttpClient httpClient, ILogger<UserApiClient> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"users/{userId}", cancellationToken).ConfigureAwait(false);
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<UserDto>(json);
        }
        
        logger.LogWarning("Failed to get user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
        response.EnsureSuccessStatusCode();
        return null;
    }
}
```

## Logging Guidelines

### Structured Logging
- Use structured logging with ILogger<T>
- Include relevant context in log messages
- Use appropriate log levels
- Avoid logging sensitive information

```csharp
public class UserService : IUserService
{
    private readonly ILogger<UserService> logger;

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating user with email: {Email}", request.Email);
        
        try
        {
            var user = await userRepository.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            
            logger.LogInformation("Successfully created user {UserId} with email: {Email}", 
                user.Id, request.Email);
            
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create user with email: {Email}", request.Email);
            throw;
        }
    }
}
```

## Performance and Resource Management

### Disposal Patterns
```csharp
public class ResourceManager : IDisposable
{
    private readonly HttpClient httpClient;
    private bool disposed = false;

    public ResourceManager(HttpClient httpClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            httpClient?.Dispose();
            disposed = true;
        }
    }
}
```

### Memory Optimization
- Use `readonly` for immutable fields
- Prefer `IEnumerable<T>` over concrete collections in method signatures
- Use `StringBuilder` for string concatenation in loops
- Consider using `Span<T>` and `Memory<T>` for performance-critical scenarios

## Code Documentation

### XML Documentation
```csharp
/// <summary>
/// Retrieves a user by their unique identifier.
/// </summary>
/// <param name="userId">The unique identifier of the user to retrieve.</param>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A <see cref="Task{UserDto}"/> representing the asynchronous operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="userId"/> is null or empty.</exception>
/// <exception cref="UserNotFoundException">Thrown when no user is found with the specified ID.</exception>
public async Task<UserDto> GetUserAsync(string userId, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrEmpty(userId))
        throw new ArgumentNullException(nameof(userId));

    // Implementation
}
```

## Security Best Practices

### Input Validation
```csharp
public async Task<UserDto> GetUserAsync(string userId, CancellationToken cancellationToken = default)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(userId))
        throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
    
    if (userId.Length > 50)
        throw new ArgumentException("User ID cannot exceed 50 characters.", nameof(userId));
    
    // Use parameterized queries for database operations
    // Never concatenate user input directly into SQL queries
}
```

### Authentication and Authorization
- Use managed identity when possible in Azure environments
- Never hardcode secrets or connection strings
- Store sensitive configuration in Azure Key Vault
- Implement proper RBAC and authorization checks

## Testing Guidelines

### Testing Frameworks
- **Unit Testing Framework**: Use xUnit for all unit tests
- **Mocking Framework**: Use Moq for creating test doubles and mocks
- **Assertions**: Use xUnit's built-in Assert class

### Unit Test Structure
```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> mockRepository;
    private readonly Mock<ILogger<UserService>> mockLogger;
    private readonly UserService userService;

    public UserServiceTests()
    {
        mockRepository = new Mock<IUserRepository>();
        mockLogger = new Mock<ILogger<UserService>>();
        userService = new UserService(mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetUserAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = "user123";
        var expectedUser = new User { Id = userId, Name = "Test User" };
        mockRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedUser);

        // Act
        var result = await userService.GetUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        mockRepository.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetUserAsync_WithInvalidId_ThrowsArgumentException(string userId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => userService.GetUserAsync(userId));
    }
}
```

## Code Formatting

### General Rules
- Use 4 spaces for indentation
- Place opening braces on new lines for types and methods
- Use inline braces for properties and simple statements
- Limit lines to 120 characters when practical
- Use blank lines to separate logical sections

### Example Formatting
```csharp
namespace XtremeIdiots.Portal.Web.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository repository;
        
        public UserService(IUserRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public string UserName { get; set; }
        
        public bool IsActive => UserName != null;

        public async Task<User> ProcessUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            var user = await repository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            
            if (user == null)
                return null;

            return ProcessUser(user);
        }

        private static User ProcessUser(User user)
        {
            // Processing logic
            return user;
        }
    }
}
```

## Avoiding Common Anti-Patterns

### Don't Do This
```csharp
// Avoid async void (except event handlers)
public async void ProcessData() { }

// Avoid blocking async calls
var result = GetDataAsync().Result;

// Avoid catching and rethrowing without adding value
try 
{
    DoSomething();
}
catch (Exception ex)
{
    throw ex; // Loses stack trace
}

// Avoid string concatenation in loops
for (int i = 0; i < items.Count; i++)
{
    result += items[i]; // Use StringBuilder instead
}
```

### Do This Instead
```csharp
// Use async Task
public async Task ProcessDataAsync() { }

// Use await
var result = await GetDataAsync().ConfigureAwait(false);

// Preserve stack trace or add context
try 
{
    DoSomething();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error processing data");
    throw; // Preserves stack trace
}

// Use StringBuilder for string building
var sb = new StringBuilder();
for (int i = 0; i < items.Count; i++)
{
    sb.Append(items[i]);
}
var result = sb.ToString();
```
