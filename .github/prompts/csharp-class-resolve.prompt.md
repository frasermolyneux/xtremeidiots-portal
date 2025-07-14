---
mode: agent
---

# C# Class Resolution Prompt

Use this prompt to automatically resolve and implement recommendations from C# class reviews based on the project's coding standards.

## Instructions

This prompt should be used **after** completing a C# class review. It will help you systematically resolve the identified issues and implement best practices.

## Resolution Framework

### Input Requirements
Before using this prompt, you should have:
1. **Original C# class code**
2. **Review feedback** with specific issues identified
3. **Priority list** of changes to implement
4. **Project context** (dependencies, frameworks used)

### Resolution Process

#### Phase 1: Critical Fixes (Security & Functionality)
Execute these changes first as they impact security or functionality:

1. **Security Issues**
   - Remove hardcoded secrets/connection strings
   - Add input validation and sanitization
   - Implement proper parameter validation
   - Add authorization checks where needed

2. **Functionality Issues**
   - Fix async/await patterns (remove `.Result`, add `ConfigureAwait(false)`)
   - Correct exception handling (use `throw;` instead of `throw ex;`)
   - Implement proper disposal patterns
   - Add missing cancellation token support

#### Phase 2: Code Standards Compliance
Implement these changes to align with coding standards:

1. **Naming Conventions**
   - Rename classes, methods, properties to PascalCase
   - Rename private fields, parameters to camelCase
   - Add "I" prefix to interfaces
   - Add "Async" suffix to async methods

2. **Structure and Organization**
   - Reorganize using statements (System → Third-party → Local)
   - Ensure single responsibility per file
   - Correct namespace hierarchy
   - Add missing XML documentation

#### Phase 3: Performance and Best Practices
Apply these optimizations and improvements:

1. **Performance Enhancements**
   - Replace string concatenation with StringBuilder
   - Add readonly modifiers where appropriate
   - Optimize collection usage
   - Implement proper resource management

2. **Code Quality**
   - Add comprehensive logging
   - Improve error messages
   - Enhance testability
   - Refactor for better maintainability

## Resolution Templates

### Template 1: Fixing Naming Conventions

```csharp
// BEFORE (Non-compliant)
public class userManager
{
    private HttpClient client;
    private ILogger logger;
    
    public async Task<User> getUser(string user_id)
    {
        // Implementation
    }
}

// AFTER (Compliant)
public class UserManager
{
    private readonly HttpClient httpClient;
    private readonly ILogger<UserManager> logger;
    
    public async Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

### Template 2: Implementing Async Best Practices

```csharp
// BEFORE (Problematic)
public class DataService
{
    public User GetUser(string id)
    {
        return GetUserAsync(id).Result; // Blocking call
    }
    
    public async void ProcessData() // async void
    {
        await SomeOperation();
    }
}

// AFTER (Fixed)
public class DataService
{
    public async Task<User> GetUserAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetUserInternalAsync(id, cancellationToken).ConfigureAwait(false);
    }
    
    public async Task ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        await SomeOperationAsync(cancellationToken).ConfigureAwait(false);
    }
}
```

### Template 3: Adding Proper Error Handling

```csharp
// BEFORE (Poor error handling)
public class UserService
{
    public async Task<User> GetUser(string id)
    {
        try
        {
            return await repository.GetAsync(id);
        }
        catch (Exception ex)
        {
            throw ex; // Loses stack trace
        }
    }
}

// AFTER (Proper error handling)
public class UserService
{
    private readonly IUserRepository repository;
    private readonly ILogger<UserService> logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{User}"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    /// <exception cref="UserNotFoundException">Thrown when no user is found with the specified ID.</exception>
    public async Task<User> GetUserAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
        }

        try
        {
            logger.LogInformation("Retrieving user with ID: {UserId}", id);
            
            var user = await repository.GetAsync(id, cancellationToken).ConfigureAwait(false);
            
            if (user == null)
            {
                logger.LogWarning("User not found with ID: {UserId}", id);
                throw new UserNotFoundException(id);
            }
            
            logger.LogDebug("Successfully retrieved user with ID: {UserId}", id);
            return user;
        }
        catch (Exception ex) when (!(ex is UserNotFoundException || ex is ArgumentException))
        {
            logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            throw;
        }
    }
}
```

### Template 4: Implementing Dependency Injection

```csharp
// BEFORE (Tight coupling)
public class OrderService
{
    private SqlConnection connection = new SqlConnection("connection_string");
    private EmailService emailService = new EmailService();
    
    public void ProcessOrder(Order order)
    {
        // Implementation
    }
}

// AFTER (Dependency injection)
public class OrderService : IOrderService
{
    private readonly IOrderRepository orderRepository;
    private readonly IEmailService emailService;
    private readonly ILogger<OrderService> logger;

    public OrderService(
        IOrderRepository orderRepository,
        IEmailService emailService,
        ILogger<OrderService> logger)
    {
        this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        logger.LogInformation("Processing order {OrderId}", order.Id);
        
        try
        {
            await orderRepository.SaveAsync(order, cancellationToken).ConfigureAwait(false);
            await emailService.SendConfirmationAsync(order.CustomerEmail, order, cancellationToken).ConfigureAwait(false);
            
            logger.LogInformation("Successfully processed order {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            throw;
        }
    }
}
```

### Template 5: Adding Comprehensive Logging

```csharp
// BEFORE (No logging)
public class UserManager
{
    public async Task<bool> CreateUser(CreateUserRequest request)
    {
        var user = new User(request.Email, request.Name);
        await repository.SaveAsync(user);
        return true;
    }
}

// AFTER (With comprehensive logging)
public class UserManager : IUserManager
{
    private readonly IUserRepository repository;
    private readonly ILogger<UserManager> logger;

    public UserManager(IUserRepository repository, ILogger<UserManager> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="request">The user creation request containing user details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{bool}"/> indicating whether the user was successfully created.</returns>
    public async Task<bool> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.", nameof(request));
        }

        logger.LogInformation("Creating user with email: {Email}", request.Email);

        try
        {
            var user = new User(request.Email, request.Name);
            await repository.SaveAsync(user, cancellationToken).ConfigureAwait(false);
            
            logger.LogInformation("Successfully created user {UserId} with email: {Email}", user.Id, request.Email);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create user with email: {Email}", request.Email);
            throw;
        }
    }
}
```

## Resolution Checklist

### ✅ Critical Fixes Applied
- [ ] Removed hardcoded secrets and connection strings
- [ ] Fixed async/await patterns (no `.Result` or `.Wait()`)
- [ ] Corrected exception handling (use `throw;`)
- [ ] Added input validation with guard clauses
- [ ] Implemented proper disposal patterns
- [ ] Added cancellation token support

### ✅ Code Standards Applied
- [ ] Applied PascalCase to classes, methods, properties
- [ ] Applied camelCase to private fields and parameters
- [ ] Added "I" prefix to interfaces
- [ ] Added "Async" suffix to async methods
- [ ] Organized using statements correctly
- [ ] Added XML documentation to public members
- [ ] Ensured single responsibility per file

### ✅ Performance & Best Practices Applied
- [ ] Added `readonly` modifiers where appropriate
- [ ] Replaced string concatenation with StringBuilder
- [ ] Used `ConfigureAwait(false)` in library code
- [ ] Implemented comprehensive logging with structured data
- [ ] Added proper constructor validation
- [ ] Optimized collection usage
- [ ] Enhanced error messages with context

### ✅ Testing & Maintainability
- [ ] Made classes easily testable (mockable dependencies)
- [ ] Reduced public surface area
- [ ] Improved method signatures for clarity
- [ ] Added meaningful variable names
- [ ] Separated concerns appropriately

## Step-by-Step Resolution Process

### 1. Analyze Review Feedback
```
Input: Review feedback with categorized issues
Output: Prioritized list of changes needed
```

### 2. Apply Critical Fixes First
```
Focus: Security, functionality, and breaking issues
Priority: High - must be fixed before other changes
```

### 3. Implement Code Standards
```
Focus: Naming, structure, documentation
Priority: Medium - improves maintainability
```

### 4. Apply Performance Optimizations
```
Focus: Resource management, async patterns
Priority: Lower - enhances performance
```

### 5. Validate Changes
```
Action: Test the changes, ensure no regressions
Check: All original functionality preserved
Verify: New standards compliance achieved
```

## Usage Instructions

1. **Start with review feedback**: Use the output from the C# class review prompt
2. **Prioritize changes**: Focus on critical issues first
3. **Apply templates**: Use the resolution templates as guides
4. **Validate incrementally**: Test after each major change
5. **Document changes**: Update any related documentation
6. **Run tests**: Ensure no functionality is broken

## Expected Outcomes

After applying this resolution prompt, your C# class should:
- ✅ **Comply with all coding standards**
- ✅ **Follow security best practices**
- ✅ **Implement proper async patterns**
- ✅ **Have comprehensive error handling**
- ✅ **Include thorough documentation**
- ✅ **Be easily testable and maintainable**
- ✅ **Demonstrate optimal performance patterns**

Remember: Always test your changes thoroughly and ensure that the refactored code maintains the same functionality while improving quality and maintainability.
