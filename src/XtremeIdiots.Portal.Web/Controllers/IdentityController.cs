using System.Security.Claims;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.XtremeIdiots;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

/// <summary>
/// Controller for managing user authentication and identity operations
/// </summary>
public class IdentityController : BaseController
{
 private readonly IXtremeIdiotsAuth xtremeIdiotsAuth;

 /// <summary>
 /// Initializes a new instance of the IdentityController
 /// </summary>
 /// <param name="xtremeIdiotsAuth">Service for handling XtremeIdiots authentication operations</param>
 /// <param name="telemetryClient">Client for tracking telemetry events</param>
 /// <param name="logger">Logger for structured logging</param>
 /// <param name="configuration">Configuration service for settings access</param>
 /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
 public IdentityController(
 IXtremeIdiotsAuth xtremeIdiotsAuth,
 TelemetryClient telemetryClient,
 ILogger<IdentityController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.xtremeIdiotsAuth = xtremeIdiotsAuth ?? throw new ArgumentNullException(nameof(xtremeIdiotsAuth));
 }

 /// <summary>
 /// Displays the login page for user authentication
 /// </summary>
 /// <param name="returnUrl">Optional URL to redirect to after successful login</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The login view with return URL context</returns>
 [HttpGet]
 [AllowAnonymous]
 public async Task<IActionResult> Login(string? returnUrl = null, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 ViewData["ReturnUrl"] = returnUrl;
 return await Task.FromResult(View());
 }, "Login", "Anonymous");
 }

 /// <summary>
 /// Initiates external authentication with XtremeIdiots OAuth provider
 /// </summary>
 /// <param name="returnUrl">Optional URL to redirect to after successful authentication</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Challenge result that Redirects to external authentication provider</returns>
 [AllowAnonymous]
 [ValidateAntiForgeryToken]
 [HttpPost]
 public async Task<IActionResult> LoginWithXtremeIdiots(string? returnUrl = null, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 var redirectUrl = Url.Action(nameof(ExternalLoginCallback), nameof(IdentityController), new { ReturnUrl = returnUrl });
 var properties = xtremeIdiotsAuth.ConfigureExternalAuthenticationProperties(redirectUrl);

 return await Task.FromResult(new ChallengeResult("XtremeIdiots", properties));
 }, "LoginWithXtremeIdiots", "Anonymous");
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
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 if (remoteError != null)
 {
 Logger.LogError("External authentication provider returned error: {RemoteError}", remoteError);
 return await IdentityError("There has been an issue logging you in with the xtremeidiots provider");
 }

 var info = await xtremeIdiotsAuth.GetExternalLoginInfoAsync();
 if (info is null)
 {
 Logger.LogWarning("External login info was null, redirecting to home");
 return RedirectToAction(nameof(HomeController.Index), nameof(HomeController));
 }

 var username = info.Principal.FindFirstValue(ClaimTypes.Name);
 var result = await xtremeIdiotsAuth.ProcessExternalLogin(info);

 switch (result)
 {
 case XtremeIdiotsAuthResult.Success:
 TrackSuccessTelemetry("UserLogin", "ExternalLoginCallback", new Dictionary<string, string>
 {
 { "Username", username ?? "Unknown" },
 { "LoginResult", "Success" }
 });

 return RedirectToLocal(returnUrl);

 case XtremeIdiotsAuthResult.Locked:
 TrackSuccessTelemetry("UserLoginLocked", "ExternalLoginCallback", new Dictionary<string, string>
 {
 { "Username", username ?? "Unknown" },
 { "LoginResult", "Locked" }
 });

 return await IdentityError("Your account is currently locked");

 default:
 Logger.LogWarning("User {Username} authentication failed with result: {Result}", username, result);

 TrackSuccessTelemetry("UserLoginFailed", "ExternalLoginCallback", new Dictionary<string, string>
 {
 { "Username", username ?? "Unknown" },
 { "LoginResult", result.ToString() }
 });

 return await IdentityError("There has been an issue logging you in");
 }
 }, "ExternalLoginCallback", "Anonymous");
 }

 /// <summary>
 /// Displays an error page with the specified message
 /// </summary>
 /// <param name="message">The error message to display to the user</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>The identity error view with the specified message</returns>
 [AllowAnonymous]
 [HttpGet]
 public async Task<IActionResult> IdentityError(string message, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 TrackSuccessTelemetry("IdentityError", "IdentityError", new Dictionary<string, string>
 {
 { "ErrorMessage", message ?? "Unknown" }
 });

 return await Task.FromResult(View(message));
 }, "IdentityError", "Anonymous");
 }

 /// <summary>
 /// Logs out the current user and Redirects to the specified URL or home page
 /// </summary>
 /// <param name="returnUrl">Optional URL to redirect to after logout</param>
 /// <param name="cancellationToken">Cancellation token for the async operation</param>
 /// <returns>Redirect to the specified return URL or home page</returns>
 [Authorize]
 [ValidateAntiForgeryToken]
 [HttpPost]
 public async Task<IActionResult> Logout(string? returnUrl = null, CancellationToken cancellationToken = default)
 {
 return await ExecuteWithErrorHandlingAsync(async () =>
 {
 await xtremeIdiotsAuth.SignOutAsync();

 TrackSuccessTelemetry("UserLogout", "Logout", new Dictionary<string, string>
 {
 { "ReturnUrl", returnUrl ?? "none" }
 });

 return RedirectToLocal(returnUrl);
 }, "Logout");
 }

 /// <summary>
 /// Redirects to the specified return URL if it's a local URL, otherwise Redirects to the home page
 /// </summary>
 /// <param name="returnUrl">The URL to redirect to if it's local</param>
 /// <returns>Redirect action result to the appropriate URL</returns>
 private IActionResult RedirectToLocal(string? returnUrl)
 {
 if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
 {
 return Redirect(returnUrl);
 }

 return RedirectToAction(nameof(HomeController.Index), nameof(HomeController));
 }
}