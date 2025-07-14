using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

using XtremeIdiots.Portal.Web.Auth.XtremeIdiots;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers
{
    /// <summary>
    /// Controller for managing user authentication and identity operations
    /// </summary>
    public class IdentityController : Controller
    {
        private readonly IXtremeIdiotsAuth xtremeIdiotsAuth;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<IdentityController> logger;

        /// <summary>
        /// Initializes a new instance of the IdentityController
        /// </summary>
        /// <param name="xtremeIdiotsAuth">Service for handling XtremeIdiots authentication operations</param>
        /// <param name="telemetryClient">Client for tracking telemetry events</param>
        /// <param name="logger">Logger for structured logging</param>
        /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
        public IdentityController(
            IXtremeIdiotsAuth xtremeIdiotsAuth,
            TelemetryClient telemetryClient,
            ILogger<IdentityController> logger)
        {
            this.xtremeIdiotsAuth = xtremeIdiotsAuth ?? throw new ArgumentNullException(nameof(xtremeIdiotsAuth));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the login page for user authentication
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The login view with return URL context</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Anonymous user accessing login page with return URL: {ReturnUrl}", returnUrl ?? "none");

                ViewData["ReturnUrl"] = returnUrl;

                logger.LogInformation("Successfully displayed login page");
                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error displaying login page with return URL: {ReturnUrl}", returnUrl ?? "none");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Initiates external authentication with XtremeIdiots OAuth provider
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after successful authentication</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Challenge result that redirects to external authentication provider</returns>
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult LoginWithXtremeIdiots(string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("User initiating login with XtremeIdiots provider, return URL: {ReturnUrl}", returnUrl ?? "none");

                var redirectUrl = Url.Action("ExternalLoginCallback", "Identity", new { ReturnUrl = returnUrl });
                var properties = xtremeIdiotsAuth.ConfigureExternalAuthenticationProperties(redirectUrl);

                logger.LogInformation("Successfully initiated XtremeIdiots authentication challenge");
                return new ChallengeResult("XtremeIdiots", properties);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initiating XtremeIdiots authentication with return URL: {ReturnUrl}", returnUrl ?? "none");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Handles the callback from external authentication provider and processes user login
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after successful authentication</param>
        /// <param name="remoteError">Optional error message from external authentication provider</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Appropriate redirect or error view based on authentication result</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Processing external login callback with return URL: {ReturnUrl}, error: {RemoteError}", returnUrl ?? "none", remoteError ?? "none");

                if (remoteError != null)
                {
                    logger.LogError("External authentication provider returned error: {RemoteError}", remoteError);
                    return IdentityError("There has been an issue logging you in with the xtremeidiots provider");
                }

                var info = await xtremeIdiotsAuth.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    logger.LogWarning("External login info was null, redirecting to home");
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                var username = info.Principal.FindFirstValue(ClaimTypes.Name);
                logger.LogInformation("User {Username} has successfully authenticated", username);

                var result = await xtremeIdiotsAuth.ProcessExternalLogin(info);

                switch (result)
                {
                    case XtremeIdiotsAuthResult.Success:
                        logger.LogInformation("User {Username} successfully logged in", username);

                        var successTelemetry = new EventTelemetry("UserLogin");
                        successTelemetry.Properties.TryAdd("Username", username ?? "Unknown");
                        successTelemetry.Properties.TryAdd("LoginResult", "Success");
                        telemetryClient.TrackEvent(successTelemetry);

                        return RedirectToLocal(returnUrl);

                    case XtremeIdiotsAuthResult.Locked:
                        logger.LogWarning("User {Username} account is locked", username);

                        var lockedTelemetry = new EventTelemetry("UserLogin");
                        lockedTelemetry.Properties.TryAdd("Username", username ?? "Unknown");
                        lockedTelemetry.Properties.TryAdd("LoginResult", "Locked");
                        telemetryClient.TrackEvent(lockedTelemetry);

                        return IdentityError("Your account is currently locked");

                    default:
                        logger.LogWarning("User {Username} authentication failed with result: {Result}", username, result);

                        var failedTelemetry = new EventTelemetry("UserLogin");
                        failedTelemetry.Properties.TryAdd("Username", username ?? "Unknown");
                        failedTelemetry.Properties.TryAdd("LoginResult", result.ToString());
                        telemetryClient.TrackEvent(failedTelemetry);

                        return IdentityError("There has been an issue logging you in");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing external login callback with return URL: {ReturnUrl}", returnUrl ?? "none");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("ReturnUrl", returnUrl ?? "none");
                exceptionTelemetry.Properties.TryAdd("RemoteError", remoteError ?? "none");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Displays an error page with the specified message
        /// </summary>
        /// <param name="message">The error message to display to the user</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>The identity error view with the specified message</returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult IdentityError(string message, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogWarning("Displaying identity error page with message: {Message}", message);

                var errorTelemetry = new EventTelemetry("IdentityError");
                errorTelemetry.Properties.TryAdd("ErrorMessage", message ?? "Unknown");
                telemetryClient.TrackEvent(errorTelemetry);

                logger.LogInformation("Successfully displayed identity error page");
                return View(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error displaying identity error page with message: {Message}", message);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("ErrorMessage", message ?? "Unknown");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Logs out the current user and redirects to the specified URL or home page
        /// </summary>
        /// <param name="returnUrl">Optional URL to redirect to after logout</param>
        /// <param name="cancellationToken">Cancellation token for the async operation</param>
        /// <returns>Redirect to the specified return URL or home page</returns>
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout(string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var username = User.Identity?.Name;
                var userId = User.XtremeIdiotsId();

                logger.LogInformation("User {UserId} ({Username}) initiating logout", userId, username);

                await xtremeIdiotsAuth.SignOutAsync();

                logger.LogInformation("User {UserId} ({Username}) successfully logged out", userId, username);

                var logoutTelemetry = new EventTelemetry("UserLogout")
                    .Enrich(User);
                telemetryClient.TrackEvent(logoutTelemetry);

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                var username = User.Identity?.Name;
                var userId = User.XtremeIdiotsId();

                logger.LogError(ex, "Error during logout for user {UserId} ({Username})", userId, username);

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Enrich(User);
                exceptionTelemetry.Properties.TryAdd("ReturnUrl", returnUrl ?? "none");
                telemetryClient.TrackException(exceptionTelemetry);

                throw;
            }
        }

        /// <summary>
        /// Redirects to the specified return URL if it's a local URL, otherwise redirects to the home page
        /// </summary>
        /// <param name="returnUrl">The URL to redirect to if it's local</param>
        /// <returns>Redirect action result to the appropriate URL</returns>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    logger.LogInformation("Redirecting user to local return URL: {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }

                logger.LogInformation("Redirecting user to home page (no valid return URL provided)");
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error redirecting to local URL: {ReturnUrl}", returnUrl ?? "none");

                var exceptionTelemetry = new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error
                };
                exceptionTelemetry.Properties.TryAdd("ReturnUrl", returnUrl ?? "none");
                telemetryClient.TrackException(exceptionTelemetry);

                // Fallback to home page on error
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}