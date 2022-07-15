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
                options.BaseUrl = Configuration["xtremeidiots_forums_base_url"];
                options.ApiKey = Configuration["xtremeidiots_forums_api_key"];
            });

            services.AddAdminActionTopics();

            services.AddScoped<IDemoManager, DemoManager>();

            services.AddRepositoryApiClient(options =>
            {
                options.BaseUrl = Configuration["repository_api_base_url"] ?? Configuration["apim_base_url"];
                options.ApiKey = Configuration["apim_subscription_key"];
                options.ApiPathPrefix = Configuration["repository_api_path_prefix"] ?? "repository";
            });

            services.AddServersApiClient(options =>
            {
                options.BaseUrl = Configuration["servers_api_base_url"] ?? Configuration["apim_base_url"];
                options.ApiKey = Configuration["apim_subscription_key"];
                options.ApiPathPrefix = Configuration["servers_api_path_prefix"] ?? "servers";
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