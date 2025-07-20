using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.Web.Areas.Identity.Data;
using XtremeIdiots.Portal.Web.Extensions;
using XtremeIdiots.Portal.Integrations.Forums;
using XtremeIdiots.Portal.Integrations.Forums.Extensions;
using XtremeIdiots.Portal.Repository.Api.Client.V1;
using XtremeIdiots.Portal.Integrations.Servers.Api.Client.V1;
using MX.GeoLocation.Api.Client.V1;

namespace XtremeIdiots.Portal.Web;

/// <summary>
/// Configures application services and the HTTP request pipeline .
/// This class handles dependency injection, authentication, authorization and middleware configuration.
/// </summary>
public class Startup
{
 /// <summary>
 /// Initializes a new instance of the <see cref="Startup"/> class.
 /// </summary>
 /// <param name="configuration">The application configuration settings</param>
 public Startup(IConfiguration configuration)
 {
 Configuration = configuration;
 }

 /// <summary>
 /// Gets the application configuration settings.
 /// </summary>
 public IConfiguration Configuration { get; }

 /// <summary>
 /// Configures application services for dependency injection.
 /// This method gets called by the runtime to add services to the container.
 /// </summary>
 /// <param name="services">The service collection to configure</param>
 public void ConfigureServices(IServiceCollection services)
 {
 // Configure Application Insights telemetry
 services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
 services.AddLogging();

 // Configure adaptive sampling to exclude exceptions per Microsoft guidance
 // https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling-classic-api#configure-sampling-settings
 services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
 {
 var telemetryProcessorChainBuilder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
 telemetryProcessorChainBuilder.UseAdaptiveSampling(excludedTypes: "Exception");
 telemetryProcessorChainBuilder.Build();
 });

 services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
 {
 EnableAdaptiveSampling = false,
 });

 services.AddServiceProfiler();

 // Configure Invision Community forum integration
 services.AddInvisionApiClient(options =>
 {
 options.BaseUrl = Configuration["xtremeidiots_forums_base_url"] ?? throw new ArgumentNullException(nameof(Configuration), "xtremeidiots_forums_base_url configuration is required");
 options.ApiKey = Configuration["xtremeidiots_forums_api_key"] ?? throw new ArgumentNullException(nameof(Configuration), "xtremeidiots_forums_api_key configuration is required");
 });

 services.AddAdminActionTopics();
 services.AddScoped<IDemoManager, DemoManager>();

 // Configure external API clients with dual authentication (API key + Entra ID)
 services.AddRepositoryApiClient(options =>
 {
 options.WithBaseUrl(Configuration["RepositoryApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(Configuration), "RepositoryApi:BaseUrl configuration is required"))
 .WithApiKeyAuthentication(Configuration["RepositoryApi:ApiKey"] ?? throw new ArgumentNullException(nameof(Configuration), "RepositoryApi:ApiKey configuration is required"))
 .WithEntraIdAuthentication(Configuration["RepositoryApi:ApplicationAudience"] ?? throw new ArgumentNullException(nameof(Configuration), "RepositoryApi:ApplicationAudience configuration is required"));
 });

 services.AddServersApiClient(options =>
 {
 options.WithBaseUrl(Configuration["ServersIntegrationApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(Configuration), "ServersIntegrationApi:BaseUrl configuration is required"))
 .WithApiKeyAuthentication(Configuration["ServersIntegrationApi:ApiKey"] ?? throw new ArgumentNullException(nameof(Configuration), "ServersIntegrationApi:ApiKey configuration is required"))
 .WithEntraIdAuthentication(Configuration["ServersIntegrationApi:ApplicationAudience"] ?? throw new ArgumentNullException(nameof(Configuration), "ServersIntegrationApi:ApplicationAudience configuration is required"));
 });

 services.AddGeoLocationApiClient(options =>
 {
 options.WithBaseUrl(Configuration["GeoLocationApi:BaseUrl"] ?? throw new ArgumentNullException(nameof(Configuration), "GeoLocationApi:BaseUrl configuration is required"))
 .WithApiKeyAuthentication(Configuration["GeoLocationApi:ApiKey"] ?? throw new ArgumentNullException(nameof(Configuration), "GeoLocationApi:ApiKey configuration is required"))
 .WithEntraIdAuthentication(Configuration["GeoLocationApi:ApplicationAudience"] ?? throw new ArgumentNullException(nameof(Configuration), "GeoLocationApi:ApplicationAudience configuration is required"));
 });

 // Configure XtremeIdiots custom authentication and authorization
 services.AddXtremeIdiotsAuth();
 services.AddAuthorization(options =>
 {
 options.AddXtremeIdiotsPolicies();
 });

 // Configure CORS for forum integration
 services.AddCors(options =>
 {
 var corsOrigin = Configuration["xtremeidiots_forums_base_url"] ?? throw new ArgumentNullException(nameof(Configuration), "xtremeidiots_forums_base_url configuration is required");
 options.AddPolicy("CorsPolicy",
 builder => builder.WithOrigins(corsOrigin)
 .AllowAnyMethod()
 .AllowAnyHeader()
 .AllowCredentials());
 });

 services.AddControllersWithViews().AddRazorRuntimeCompilation();

 // Configure essential cookies for TempData
 services.Configure<CookieTempDataProviderOptions>(options =>
 {
 options.Cookie.IsEssential = true;
 });

 // Configure additional services
 services.AddHttpClient();
 services.AddMemoryCache();
 services.AddScoped<Services.IProxyCheckService, Services.ProxyCheckService>();

 // Configure forwarded headers for proxy scenarios
 services.Configure<ForwardedHeadersOptions>(options =>
 {
 options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
 options.KnownNetworks.Clear();
 options.KnownProxies.Clear();
 });

 services.AddHealthChecks();
 }

 /// <summary>
 /// Configures the HTTP request pipeline and middleware.
 /// This method gets called by the runtime to configure the HTTP request pipeline.
 /// </summary>
 /// <param name="app">The application builder to configure</param>
 /// <param name="env">The web host environment</param>
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
 {
 // Enable forwarded headers for proxy scenarios (must be first)
 app.UseForwardedHeaders();

 // Configure error handling based on environment
 if (env.IsDevelopment())
 {
 app.UseDeveloperExceptionPage();
 }
 else
 {
 app.UseExceptionHandler("/Errors/Display/500");
 app.UseHsts();
 }

 // Core middleware pipeline
 app.UseHttpsRedirection();
 app.UseStaticFiles();
 app.UseCookiePolicy();
 app.UseRouting();

 // Authentication and authorization middleware
 app.UseCors();
 app.UseAuthentication();
 app.UseAuthorization();

 // Custom error page handling
 app.UseStatusCodePagesWithRedirects("/Errors/Display/{0}");

 // Configure endpoint routing
 app.UseEndpoints(endpoints =>
 {
 // API Controller routes - configured with explicit routing
 endpoints.MapControllers();

 // Traditional MVC routes
 endpoints.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");
 });

 // Health check endpoint
 app.UseHealthChecks(new PathString("/api/health"));

 // Perform database migration on startup
 using var scope = app.ApplicationServices.CreateScope();
 var identityDataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
 identityDataContext.Database.Migrate();
 }
}