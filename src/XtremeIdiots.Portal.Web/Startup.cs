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

namespace XtremeIdiots.Portal.Web
{
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

            //https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling-classic-api#configure-sampling-settings
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
                options.BaseUrl = Configuration["xtremeidiots_forums_base_url"];
                options.ApiKey = Configuration["xtremeidiots_forums_api_key"];
            });

            services.AddAdminActionTopics();

            services.AddScoped<IDemoManager, DemoManager>();

            services.AddRepositoryApiClient(options =>
            {
                options.WithBaseUrl(Configuration["RepositoryApi:BaseUrl"] ?? throw new ArgumentNullException("RepositoryApi:BaseUrl"))
                    .WithApiKeyAuthentication(Configuration["RepositoryApi:ApiKey"] ?? throw new ArgumentNullException("RepositoryApi:ApiKey"))
                    .WithEntraIdAuthentication(Configuration["RepositoryApi:ApplicationAudience"] ?? throw new ArgumentNullException("RepositoryApi:ApplicationAudience"));
            });

            services.AddServersApiClient(options =>
            {
                options.WithBaseUrl(Configuration["ServersIntegrationApi:BaseUrl"] ?? throw new ArgumentNullException("ServersIntegrationApi:BaseUrl"))
                    .WithApiKeyAuthentication(Configuration["ServersIntegrationApi:ApiKey"] ?? throw new ArgumentNullException("ServersIntegrationApi:ApiKey"))
                    .WithEntraIdAuthentication(Configuration["ServersIntegrationApi:ApplicationAudience"] ?? throw new ArgumentNullException("ServersIntegrationApi:ApplicationAudience"));
            });

            services.AddGeoLocationApiClient(options =>
            {
                options.WithBaseUrl(Configuration["GeoLocationApi:BaseUrl"] ?? throw new ArgumentNullException("GeoLocationApi:BaseUrl"))
                    .WithApiKeyAuthentication(Configuration["GeoLocationApi:ApiKey"] ?? throw new ArgumentNullException("GeoLocationApi:ApiKey"))
                    .WithEntraIdAuthentication(Configuration["GeoLocationApi:ApplicationAudience"] ?? throw new ArgumentNullException("GeoLocationApi:ApplicationAudience"));
            });

            services.AddXtremeIdiotsAuth();
            services.AddAuthorization(options => { options.AddXtremeIdiotsPolicies(); });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(Configuration["xtremeidiots_forums_base_url"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.Configure<CookieTempDataProviderOptions>(options => { options.Cookie.IsEssential = true; });

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
                app.UseDeveloperExceptionPage();
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
                // API Controller routes - configured with explicit routing
                endpoints.MapControllers();

                // Traditional MVC routes
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseHealthChecks(new PathString("/api/health"));

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var identityDataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
                identityDataContext.Database.Migrate();
            }
        }
    }
}