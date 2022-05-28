using FM.GeoLocation.Client.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Web.Areas.Identity.Data;
using XI.Portal.Web.Extensions;
using XtremeIdiots.Portal.ForumsIntegration;
using XtremeIdiots.Portal.ForumsIntegration.Extensions;
using XtremeIdiots.Portal.InvisionApiClient;
using XtremeIdiots.Portal.RepositoryApiClient;
using XtremeIdiots.Portal.ServersApiClient;

namespace XI.Portal.Web
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

            services.AddGeoLocationClient(options =>
            {
                options.BaseUrl = Configuration["geolocation-baseurl"];
                options.ApiKey = Configuration["geolocation-apikey"];
                options.UseMemoryCache = true;
                options.BubbleExceptions = false;
                options.CacheEntryLifeInMinutes = 60;
                options.RetryTimespans = new[]
                {
                    TimeSpan.FromSeconds(1)
                };
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
                options.ApimBaseUrl = Configuration["apim-base-url"];
                options.ApimSubscriptionKey = Configuration["apim-subscription-key"];
            });

            services.AddServersApiClient(options =>
            {
                options.ApimBaseUrl = Configuration["apim-base-url"];
                options.ApimSubscriptionKey = Configuration["apim-subscription-key"];
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