using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MX.GeoLocation.GeoLocationApi.Client;

using XtremeIdiots.InvisionCommunity;
using XtremeIdiots.Portal.AdminWebApp.Areas.Identity.Data;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.ForumsIntegration.Extensions;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApiClient;

namespace XtremeIdiots.Portal.AdminWebApp
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

            services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
            {
                var builder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;

                // Using fixed rate sampling
                double fixedSamplingPercentage = 50;
                builder.UseSampling(fixedSamplingPercentage);
            });

            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
            });

            services.AddGeoLocationApiClient(options =>
            {
                options.BaseUrl = Configuration["apim_base_url"] ?? Configuration["geolocation_base_url"];
                options.ApiKey = Configuration["geolocation_apim_subscription_key"];
            });

            services.AddInvisionApiClient(options =>
            {
                options.BaseUrl = Configuration["xtremeidiots_forums_base_url"];
                options.ApiKey = Configuration["xtremeidiots_forums_api_key"];
            });

            services.AddAdminActionTopics();

            services.AddScoped<IDemoManager, DemoManager>();

            services.AddRepositoryApiClient(options => new RepositoryApiClientOptions(
                Configuration["apim_base_url"] ?? Configuration["repository_base_url"] ?? throw new ArgumentNullException("apim_base_url"),
                Configuration["portal_repository_apim_subscription_key"] ?? throw new ArgumentNullException("portal_repository_apim_subscription_key"),
                Configuration["repository_api_application_audience"] ?? throw new ArgumentNullException("repository_api_application_audience"),
                Configuration["repository_api_path_prefix"] ?? "repository")
            );

            services.AddServersApiClient(options => new ServersApiClientOptions(
                Configuration["apim_base_url"] ?? Configuration["servers_base_url"] ?? throw new ArgumentNullException("apim_base_url"),
                Configuration["portal_servers_apim_subscription_key"] ?? throw new ArgumentNullException("portal_servers_apim_subscription_key"),
                Configuration["servers_api_application_audience"] ?? throw new ArgumentNullException("servers_api_application_audience"),
                Configuration["servers_api_path_prefix"] ?? "servers")
            );

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

            services.AddMemoryCache();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
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
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var identityDataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
                identityDataContext.Database.Migrate();
            }
        }
    }
}