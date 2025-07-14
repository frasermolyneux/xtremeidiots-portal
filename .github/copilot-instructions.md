# XtremeIdiots Portal - AI Coding Guide

## Architecture Overview

This is a **gaming community management portal** built as an ASP.NET Core MVC web application for managing game servers, players, and admin actions across multiple Call of Duty games (COD2, COD4, COD5).

### Core Components
- **Web Portal** (`XtremeIdiots.Portal.Web`) - Main MVC application with admin interfaces
- **Forums Integration** (`XtremeIdiots.Portal.Integrations.Forums`) - Invision Community forum integration
- **External APIs** - Repository API, Servers API, GeoLocation API (via NuGet packages)
- **Infrastructure** - Azure App Service with SQL Server, deployed via Bicep templates

## Key Patterns & Conventions

### Authorization Architecture
- **Policy-based authorization** with granular permissions per controller action
- Auth policies defined in `Extensions/PolicyExtensions.cs` using custom requirements
- Authorization handlers in `Auth/Handlers/` directory implement specific business logic
- Example: `AuthPolicies.AccessPlayers`, `AuthPolicies.CreateAdminAction`, etc.
- Always use `[Authorize(Policy = AuthPolicies.YourPolicy)]` on controllers/actions

### API Client Integration Pattern
```csharp
// In Startup.cs - fluent configuration with dual auth (API key + Entra ID)
services.AddRepositoryApiClient(options =>
{
    options.WithBaseUrl(Configuration["RepositoryApi:BaseUrl"])
        .WithApiKeyAuthentication(Configuration["RepositoryApi:ApiKey"])
        .WithEntraIdAuthentication(Configuration["RepositoryApi:ApplicationAudience"]);
});
```

### Game-Specific Constants
- Use `GameType` enum from `XtremeIdiots.Portal.Repository.Abstractions.Constants.V1`
- Supported games defined in `Constants/` directory (e.g., `ChatLogSupportedGames.cs`)
- Game-specific features controlled by these constants

### ViewModels & Data Flow
- **ViewModels** contain presentation logic and enriched data (e.g., `PlayerDetailsViewModel`)
- **Enrichment pattern**: Combine API data with external services (geo-location, proxy checks)
- Controllers aggregate data from multiple API clients before passing to views

### Extension Methods
- Heavy use of extension methods for HTML helpers, DTOs, and service configuration
- Located in `Extensions/` directory with specific naming: `{Type}Extensions.cs`
- Example: `PlayerHtmlExtensions.cs` for player-specific display logic

## Development Workflows

### Build & Test Commands
```bash
# Clean and build the main web project
dotnet clean src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
dotnet build src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj

# Run tests (excludes IntegrationTests)
dotnet test src --filter "FullyQualifiedName!~IntegrationTests"

# Watch mode for development
dotnet watch run --project src/XtremeIdiots.Portal.Web

# Release build and publish
dotnet clean --configuration Release src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
dotnet publish --configuration Release src/XtremeIdiots.Portal.Web/XtremeIdiots.Portal.Web.csproj
```

### Configuration Management
- **Base config**: `appsettings.json` with storage table names and external service URLs
- **Dev overrides**: `appsettings.Development.json`
- **Secrets**: User Secrets for local dev, Azure Key Vault for production
- **API endpoints**: Configure via `{ServiceName}Api:BaseUrl` pattern

### Database & Migrations
- **Identity Database**: Entity Framework with auto-migration in `Startup.cs`
- **Main data**: External Repository API (separate service)
- **Storage**: Mix of SQL Server (identity) and Azure Table Storage (logs, demos)

## Integration Points

### External Dependencies
- **Forums**: Invision Community API via `XtremeIdiots.InvisionCommunity` package
- **Game Servers**: Custom Servers Integration API for RCON, FTP operations
- **Geo Services**: MX.GeoLocation API for IP address enrichment
- **Project References**: `portal-repository` for data access abstractions

### Cross-Service Communication
- All external APIs use **dual authentication** (API key + Entra ID tokens)
- **Telemetry**: Application Insights with custom telemetry client injection
- **Health Checks**: Available at `/api/health` endpoint

## Infrastructure & Deployment

### Azure Resources (via Bicep)
- **App Service**: Main web application host
- **Key Vault**: Configuration and secrets management
- **Application Insights**: Monitoring and telemetry
- **SQL Server**: Identity and session data

### Deployment Pipeline
- **Azure DevOps**: Primary CI/CD in `.azure-pipelines/`
- **GitHub Actions**: Code quality and feature development in `.github/workflows/`
- **Blue-Green**: Supported via `blueGreenDeploy` parameter in templates
- **Environments**: dev, prd with parameter files in `params/`

## Coding Conventions

### Error Handling
- Use structured logging with `ILogger<T>`
- Application Insights for telemetry tracking
- StatusCodePages middleware redirects errors to `/Errors/Display/{statusCode}`

### Security Practices
- **CORS**: Configured for forums integration only
- **Headers**: ForwardedHeaders middleware for proxy scenarios
- **Data Protection**: Entity Framework-backed for distributed scenarios
- **Identity**: ASP.NET Core Identity with custom claims handling

### Performance Patterns
- **Memory caching**: `IMemoryCache` for frequently accessed data
- **HTTP clients**: Configured via DI, not direct instantiation
- **Telemetry sampling**: Adaptive sampling enabled in Application Insights
