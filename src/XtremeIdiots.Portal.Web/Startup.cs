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

public class Startup
{

 public Startup(IConfiguration configuration)
 {
 Configuration = configuration;
 }

 public IConfiguration Configuration { get; }

 public void ConfigureServices(IServiceCollection services)
 {

 services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
 services.AddLogging();

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

 services.AddInvisionApiClient(options =>
 {
 options.BaseUrl = Configuration["xtremeidiots_forums_base_url"] ?? throw new ArgumentNullException(nameof(Configuration), "xtremeidiots_forums_base_url configuration is required");
 options.ApiKey = Configuration["xtremeidiots_forums_api_key"] ?? throw new ArgumentNullException(nameof(Configuration), "xtremeidiots_forums_api_key configuration is required");
 });

 services.AddAdminActionTopics();
 services.AddScoped<IDemoManager, DemoManager>();

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

 services.AddXtremeIdiotsAuth();
 services.AddAuthorization(options =>
 {
 options.AddXtremeIdiotsPolicies();
 });

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

 services.Configure<CookieTempDataProviderOptions>(options =>
 {
 options.Cookie.IsEssential = true;
 });

 services.AddHttpClient();
 services.AddMemoryCache();
 services.AddScoped<Services.IProxyCheckService, Services.ProxyCheckService>();

 services.Configure<ForwardedHeadersOptions>(options =>
 {
 options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
 options.KnownNetworks.Clear();
 options.KnownProxies.Clear();
 });

 services.AddHealthChecks();
 }

 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
 {

 app.UseForwardedHeaders();

 if (env.IsDevelopment())
 {
 app.UseDeveloperExceptionPage();
 }
 else
 {
 app.UseExceptionHandler("/Errors/Display/500");
 app.UseHsts();
 }

 app.UseHttpsRedirection();
 app.UseStaticFiles();
 app.UseCookiePolicy();
 app.UseRouting();

 app.UseCors();
 app.UseAuthentication();
 app.UseAuthorization();

 app.UseStatusCodePagesWithRedirects("/Errors/Display/{0}");

 app.UseEndpoints(endpoints =>
 {

 endpoints.MapControllers();

 endpoints.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");
 });

 app.UseHealthChecks(new PathString("/api/health"));

 using var scope = app.ApplicationServices.CreateScope();
 var identityDataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
 identityDataContext.Database.Migrate();
 }
}