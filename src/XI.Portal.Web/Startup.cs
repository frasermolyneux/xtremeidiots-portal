using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using FM.GeoLocation.Client.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XI.AzureTableLogging.Extensions;
using XI.Forums.Extensions;
using XI.Portal.Auth.Contract.Models;
using XI.Portal.Auth.Extensions;
using XI.Portal.Auth.XtremeIdiots;
using XI.Portal.Data.Legacy;
using XI.Portal.Demos.Extensions;
using XI.Portal.Maps.Extensions;
using XI.Portal.Players.Extensions;
using XI.Portal.Servers.Extensions;
using XI.Portal.Users.Data;
using XI.Portal.Users.Extensions;
using IdentityRole = ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole;

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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddIdentity<PortalIdentityUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = string.Empty;
                })
                .AddAzureTableStores<ApplicationAuthDbContext>(() =>
                {
                    var config = new IdentityConfiguration
                    {
                        StorageConnectionString = Configuration["ApplicationAuthDbContext:StorageConnectionString"],
                        LocationMode = "PrimaryOnly",
                        IndexTableName = "AspNetIndex",
                        RoleTableName = "AspNetRoles",
                        UserTableName = "AspNetUsers",
                        TablePrefix = ""
                    };
                    return config;
                })
                .AddDefaultTokenProviders()
                .CreateAzureTablesIfNotExists<ApplicationAuthDbContext>();

            services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.FromMinutes(15); });

            services.AddAuthentication(options => options.DefaultChallengeScheme = "XtremeIdiots")
                .AddOAuth("XtremeIdiots", options =>
                {
                    options.ClientId = Configuration["XtremeIdiotsAuth:ClientId"];
                    options.ClientSecret = Configuration["XtremeIdiotsAuth:ClientSecret"];
                    options.CallbackPath = new PathString("/signin-xtremeidiots");

                    options.AuthorizationEndpoint = "https://www.xtremeidiots.com/oauth/authorize/";
                    options.TokenEndpoint = "https://www.xtremeidiots.com/oauth/token/";
                    options.UserInformationEndpoint = "https://www.xtremeidiots.com/api/core/me";

                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                    options.Scope.Add("profile");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request =
                                new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request,
                                HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var contentAsString = await response.Content.ReadAsStringAsync();
                            var user = JsonDocument.Parse(contentAsString);

                            context.RunClaimActions(user.RootElement);
                        }
                    };
                });

            services.AddLogging(
                logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddAzureTableLogger(options =>
                    {
                        options.CreateTableIfNotExists = true;
                        options.StorageTableName = Configuration["Logging:AzureTableLogger:StorageTableName"];
                        options.StorageConnectionString = Configuration["AppDataContainer:StorageConnectionString"];
                    });
                });

            services.AddGeoLocationClient(options =>
            {
                options.BaseUrl = Configuration["GeoLocation:BaseUrl"];
                options.ApiKey = Configuration["GeoLocation:ApiKey"];
                options.UseMemoryCache = true;
                options.CacheEntryLifeInMinutes = 60;
                options.RetryTimespans = new[]
                {
                    TimeSpan.FromSeconds(1)
                };
            });

            services.AddForumsClient(options =>
            {
                options.BaseUrl = Configuration["XtremeIdiotsForums:BaseUrl"];
                options.ApiKey = Configuration["XtremeIdiotsForums:ApiKey"];
            });

            services.AddMapsModule(options =>
            {
                options.ConfigureMapFileRepository(repositoryOptions => { repositoryOptions.MapRedirectBaseUrl = Configuration["MapsRedirect:BaseUrl"]; });

                options.ConfigureMapImageRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = Configuration["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageContainerName = Configuration["MapImageCache:StorageContainerName"];
                });

                options.ConfigureMapsRepository(repositoryOptions => { repositoryOptions.MapRedirectBaseUrl = Configuration["MapsRedirect:BaseUrl"]; });
            });

            services.AddDemosModule(options =>
            {
                options.ConfigureDemosRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = Configuration["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageContainerName = Configuration["DemosRepository:StorageContainerName"];
                });

                options.ConfigureDemoAuthRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = Configuration["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = Configuration["DemoAuthRepository:StorageTableName"];
                });
            });

            services.AddPlayersModule(options =>
            {
                options.ConfigurePlayersRepository(repositoryOptions => { });
                options.ConfigureAdminActionsRepository(repositoryOptions => { });
                options.ConfigurePlayerLocationsRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = Configuration["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = Configuration["PlayerLocationsRepository:StorageTableName"];
                });
            });

            services.AddUsersModule(options =>
            {
                options.ConfigureUsersRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = Configuration["ApplicationAuthDbContext:StorageConnectionString"];
                    repositoryOptions.StorageTableName = Configuration["ApplicationAuthDbContext:PortalClaimsStorageTableName"];
                });
            });

            services.AddXtremeIdiotsAuth();
            services.AddAuthorization(options => { options.AddXtremeIdiotsPolicies(); });

            services.AddServersModule(options =>
            {
                options.ConfigureGameServersRepository(repositoryOptions => { });
                options.ConfigureBanFileMonitorsRepository(repositoryOptions => { });
                options.ConfigureFileMonitorsRepository(repositoryOptions => { });
                options.ConfigureRconMonitorsRepository(repositoryOptions => { });
                options.ConfigureGameServerStatusRepository(repositoryOptions =>
                {
                    repositoryOptions.StorageConnectionString = Configuration["AppDataContainer:StorageConnectionString"];
                    repositoryOptions.StorageTableName = Configuration["GameServerStatusRepository:StorageTableName"];
                });
                options.ConfigureChatLogsRepository(repositoryOptions => { });
            });

            services.AddDbContext<LegacyPortalContext>(options =>
                options.UseSqlServer(Configuration["LegacyPortalContext:ConnectionString"]));

            services.AddControllersWithViews();

            services.Configure<CookieTempDataProviderOptions>(options => { options.Cookie.IsEssential = true; });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}