using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

using XtremeIdiots.Portal.AdminWebApp.Areas.Identity;
using XtremeIdiots.Portal.AdminWebApp.Areas.Identity.Data;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace XtremeIdiots.Portal.AdminWebApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<IdentityDataContext>(options =>
                    options.UseSqlServer(context.Configuration["sql_connection_string"]));

                services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.User.RequireUniqueEmail = true;
                    options.User.AllowedUserNameCharacters = string.Empty;
                }).AddDefaultTokenProviders()
                .AddEntityFrameworkStores<IdentityDataContext>();

                services.Configure<CookiePolicyOptions>(options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                    options.Secure = CookieSecurePolicy.Always;
                });

                services.Configure<SecurityStampValidatorOptions>(options => { options.ValidationInterval = TimeSpan.FromMinutes(15); });

                services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.AccessDeniedPath = "/Errors/Display/401";
                        options.Cookie.Name = "XIPortal";
                        options.Cookie.HttpOnly = true;
                        options.ExpireTimeSpan = TimeSpan.FromDays(7);
                        options.LoginPath = "/Identity/Login";
                        options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                        options.SlidingExpiration = true;
                    })
                    .AddOAuth("XtremeIdiots", options =>
                    {
                        options.ClientId = context.Configuration["xtremeidiots_auth_client_id"];
                        options.ClientSecret = context.Configuration["xtremeidiots_auth_client_secret"];

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
                                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                                response.EnsureSuccessStatusCode();

                                var contentAsString = await response.Content.ReadAsStringAsync();
                                var user = JsonDocument.Parse(contentAsString);

                                context.RunClaimActions(user.RootElement);
                            }
                        };
                    });

                services.AddDataProtection()
                    .PersistKeysToDbContext<IdentityDataContext>();
            });
        }
    }
}