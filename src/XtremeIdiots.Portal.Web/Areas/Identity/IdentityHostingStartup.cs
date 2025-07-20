using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

using XtremeIdiots.Portal.Web.Areas.Identity;
using XtremeIdiots.Portal.Web.Areas.Identity.Data;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace XtremeIdiots.Portal.Web.Areas.Identity
{

    public class IdentityHostingStartup : IHostingStartup
    {
        private const int SecurityStampValidationIntervalMinutes = 15;
        private const int CookieExpirationDays = 7;
        private const string ApplicationName = "portal";
        private const string CookieName = "XIPortal";
        private const string OAuthSchemeName = "XtremeIdiots";

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                ValidateConfiguration(context.Configuration);
                ConfigureDatabase(services, context.Configuration);
                ConfigureIdentity(services);
                ConfigureCookiePolicy(services);
                ConfigureAuthentication(services, context.Configuration);
                ConfigureDataProtection(services);
            });
        }

        private static void ValidateConfiguration(IConfiguration configuration)
        {
            var requiredKeys = new[]
            {
                "xtremeidiots_auth_client_id",
                "xtremeidiots_auth_client_secret",
                "sql_connection_string"
            };

            foreach (var key in requiredKeys)
            {
                if (string.IsNullOrEmpty(configuration[key]))
                {
                    throw new InvalidOperationException($"Required configuration key '{key}' is missing or empty");
                }
            }
        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityDataContext>(options =>
                options.UseSqlServer(configuration["sql_connection_string"]));
        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = string.Empty;
            }).AddDefaultTokenProviders()
            .AddEntityFrameworkStores<IdentityDataContext>();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromMinutes(SecurityStampValidationIntervalMinutes);
            });
        }

        private static void ConfigureCookiePolicy(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });
        }

        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OAuthSchemeName;
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/Errors/Display/401";
                options.Cookie.Name = CookieName;
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(CookieExpirationDays);
                options.LoginPath = "/Identity/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            })
            .AddOAuth(OAuthSchemeName, options =>
            {
                options.ClientId = configuration["xtremeidiots_auth_client_id"] ?? throw new InvalidOperationException("OAuth client ID is required");
                options.ClientSecret = configuration["xtremeidiots_auth_client_secret"] ?? throw new InvalidOperationException("OAuth client secret is required");

                options.CallbackPath = new PathString("/signin-xtremeidiots");

                options.AuthorizationEndpoint = configuration["xtremeidiots_auth_authorization_endpoint"] ?? "https://www.xtremeidiots.com/oauth/authorize/";
                options.TokenEndpoint = configuration["xtremeidiots_auth_token_endpoint"] ?? "https://www.xtremeidiots.com/oauth/token/";
                options.UserInformationEndpoint = configuration["xtremeidiots_auth_userinfo_endpoint"] ?? "https://www.xtremeidiots.com/api/core/me";

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                options.Scope.Add("profile");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        try
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted).ConfigureAwait(false);
                            response.EnsureSuccessStatusCode();

                            var contentAsString = await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted).ConfigureAwait(false);
                            var user = JsonDocument.Parse(contentAsString);

                            context.RunClaimActions(user.RootElement);
                        }
                        catch (HttpRequestException ex)
                        {
                            throw new InvalidOperationException("Failed to retrieve user information from OAuth provider", ex);
                        }
                        catch (JsonException ex)
                        {
                            throw new InvalidOperationException("Failed to parse user information from OAuth provider", ex);
                        }
                    }
                };
            });
        }

        private static void ConfigureDataProtection(IServiceCollection services)
        {
            services.AddDataProtection()
                .SetApplicationName(ApplicationName)
                .PersistKeysToDbContext<IdentityDataContext>();
        }
    }
}