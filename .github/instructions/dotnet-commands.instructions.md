---
description: 'Guidelines for executing dotnet commands for build and test verification'
applyTo: '*'
---

# .NET Command Execution Guidelines

## Core Principle
**Always execute dotnet commands directly using the terminal**

When working with .NET projects, Copilot should immediately verify code changes by running the appropriate dotnet commands to ensure the code compiles and tests pass.

Do *NOT* use the processes defined in the `tasks.json` to execute commands. Instead, use the terminal to run `dotnet build`, `dotnet test`, and other relevant commands directly.

## Required Command Execution Pattern

### Build Verification
- **ALWAYS** run `dotnet build` after making code changes to verify compilation
- Use the project-specific build command when available: `dotnet build src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj`
- For solution-wide builds, use: `dotnet build src/XtremeIdiots.Portal.Web.sln`

### Razor View Validation
- **For Razor view changes**: Run Razor-specific validation to catch view compilation errors
- Use `dotnet build --configuration Release` to validate Razor views with build-time compilation
- Use custom validation target: `dotnet build -p:ValidateRazor=true` for explicit Razor validation
- **Development**: Debug builds use runtime compilation for fast iteration
- **Production**: Release builds use build-time compilation for performance and reliability

### Test Verification  
- **ALWAYS** run `dotnet test` after making code changes that could affect functionality
- Use the test filter to exclude integration tests: `dotnet test src --filter "FullyQualifiedName!~IntegrationTests"`
- Run tests at the solution level to ensure all affected projects are tested

### Clean Operations
- Run `dotnet clean` before builds when troubleshooting compilation issues
- Use clean commands for both debug and release configurations as needed

## Command Execution Order

1. **After Code Changes**: Immediately run build verification
2. **After Razor View Changes**: Run Razor validation with Release build or custom target
3. **After Logic Changes**: Run test verification to ensure functionality is preserved
4. **Before Completion**: Ensure both build and tests pass before concluding the task

## Examples

### Standard Workflow
```bash
# After making code changes to a controller
dotnet build src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj

# After making changes that affect business logic
dotnet test src --filter "FullyQualifiedName!~IntegrationTests"
```

### Razor View Changes Workflow
```bash
# After making changes to Razor views (.cshtml files)
dotnet build --configuration Debug src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj

# Validate Razor views for production readiness
dotnet build --configuration Release src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj

# Or use explicit Razor validation
dotnet build src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj -p:ValidateRazor=true
```

### Troubleshooting Workflow
```bash
# If build fails, clean and retry
dotnet clean src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
dotnet build src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
```

### Release Verification
```bash
# For production-ready code
dotnet clean --configuration Release src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
dotnet build --configuration Release src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
```

### Razor Compilation Strategy Verification
```bash
# Development builds (with runtime compilation for fast iteration)
dotnet build --configuration Debug src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj

# Production builds (with build-time compilation for performance)
dotnet build --configuration Release src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj

# Explicit Razor validation (catches view compilation errors)
dotnet build src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj -p:ValidateRazor=true
```

## Error Handling

### Build Errors
- If build fails, analyze the error output and fix the issues
- Re-run the build command after fixes
- Do not proceed with additional changes until build succeeds

### Test Failures
- If tests fail, analyze the failure output
- Fix the failing tests or update the code to make tests pass
- Re-run tests after fixes
- Do not conclude the task with failing tests

## Quality Gates

### Before Task Completion
- [ ] Code compiles successfully (`dotnet build` passes)
- [ ] All tests pass (`dotnet test` with appropriate filters passes)
- [ ] No compilation warnings introduced (unless explicitly acceptable)
- [ ] Code formatting and style guidelines are followed

### For Critical Changes
- [ ] Run both Debug and Release builds
- [ ] Run full test suite including integration tests if time permits
- [ ] Verify changes work in the broader application context

## Anti-Patterns to Avoid

❌ **DON'T suggest commands without running them:**
```markdown
You can run `dotnet build` to verify the changes.
```

✅ **DO run commands immediately:**
```markdown
Let me build the project to verify the changes work correctly.
[Runs dotnet build command]
```

❌ **DON'T skip verification steps:**
```markdown
The code looks correct and should compile fine.
```

✅ **DO verify with actual execution:**
```markdown
Let me verify this compiles correctly.
[Runs dotnet build command and shows results]
```

## Project-Specific Considerations

### XtremeIdiots Portal
- Use the specific project path: `src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj`
- Exclude integration tests: `--filter "FullyQualifiedName!~IntegrationTests"`
- Consider the Controllers pattern when testing controller changes
- Verify authorization and authentication changes thoroughly

### Multi-Project Solutions
- Build at the solution level when changes affect multiple projects
- Run tests across all affected projects
- Pay attention to project dependencies and build order

## Performance Considerations

- Use targeted builds (`dotnet build project.csproj`) instead of solution builds when only one project is affected
- Use incremental builds by avoiding unnecessary clean operations
- Leverage VS Code tasks which may have optimized configurations

## Summary

**Execute, don't suggest.** Always run dotnet commands directly to verify that code changes work correctly. This ensures code quality, prevents compilation errors from reaching users, and maintains the reliability of the XtremeIdiots Portal codebase.
