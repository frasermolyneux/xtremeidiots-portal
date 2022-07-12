using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using MX.GeoLocation.GeoLocationApi.Client;

using XtremeIdiots.Portal.AdminWebApp.Areas.Identity.Data;
using XtremeIdiots.Portal.AdminWebApp.Extensions;
using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.ForumsIntegration.Extensions;
using XtremeIdiots.Portal.InvisionApiClient;
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
            services.AddApplicationInsightsTelemetry();

            services.AddGeoLocationApiClient(options =>
            {
                options.ApimBaseUrl = Configuration["geolocation_apim_base_url"];
                options.ApimSubscriptionKey = Configuration["geolocation_apim_subscription_key"];
            });

            services.AddInvisionApiClient(options =>
            {
                options.BaseUrl = Configuration["xtremeidiots-forums-base-url"];
                options.ApiKey = Configuration["xtremeidiots-forums-api-key"];
            });

            services.AddAdminActionTopics();

            services.AddScoped<IDemoManager, DemoManager>();

            services.AddRepositoryApiClient(options =>
            {
                options.ApimBaseUrl = Configuration["repository-api-base-url"] ?? Configuration["apim-base-url"];
                options.ApimSubscriptionKey = Configuration["apim-subscription-key"];
                options.ApiPathPrefix = Configuration["repository-api-path-prefix"] ?? "repository";
            });

            services.AddServersApiClient(options =>
            {
                options.ApimBaseUrl = Configuration["servers-api-base-url"] ?? Configuration["apim-base-url"];
                options.ApimSubscriptionKey = Configuration["apim-subscription-key"];
                options.ApiPathPrefix = Configuration["servers-api-path-prefix"] ?? "servers";
            });

            services.AddXtremeIdiotsAuth();
            services.AddAuthorization(options => { options.AddXtremeIdiotsPolicies(); });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins(Configuration["xtremeidiots-forums-base-url"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.Configure<CookieTempDataProviderOptions>(options => { options.Cookie.IsEssential = true; });

            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Errors/Display/500");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy();

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