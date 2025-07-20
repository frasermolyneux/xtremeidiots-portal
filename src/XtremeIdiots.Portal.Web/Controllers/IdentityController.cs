using System.Security.Claims;

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using XtremeIdiots.Portal.Web.Auth.XtremeIdiots;
using XtremeIdiots.Portal.Web.Extensions;

namespace XtremeIdiots.Portal.Web.Controllers;

public class IdentityController : BaseController
{
 private readonly IXtremeIdiotsAuth xtremeIdiotsAuth;

 public IdentityController(
 IXtremeIdiotsAuth xtremeIdiotsAuth,
 TelemetryClient telemetryClient,
 ILogger<IdentityController> logger,
 IConfiguration configuration)
 : base(telemetryClient, logger, configuration)
 {
 this.xtremeIdiotsAuth = xtremeIdiotsAuth ?? throw new ArgumentNullException(nameof(xtremeIdiotsAuth));
 }

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

 private IActionResult RedirectToLocal(string? returnUrl)
 {
 if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
 {
 return Redirect(returnUrl);
 }

 return RedirectToAction(nameof(HomeController.Index), nameof(HomeController));
 }
}