using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using XI.Portal.Auth.XtremeIdiots;

namespace XI.Portal.Web.Controllers
{
    public class IdentityController : Controller
    {
        private readonly ILogger<IdentityController> _logger;
        private readonly IXtremeIdiotsAuth _xtremeIdiotsAuth;

        public IdentityController(ILogger<IdentityController> logger, IXtremeIdiotsAuth xtremeIdiotsAuth)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xtremeIdiotsAuth = xtremeIdiotsAuth ?? throw new ArgumentNullException(nameof(xtremeIdiotsAuth));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult LoginWithXtremeIdiots(string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Identity", new { ReturnUrl = returnUrl });
            var properties = _xtremeIdiotsAuth.ConfigureExternalAuthenticationProperties(redirectUrl);
            return new ChallengeResult("XtremeIdiots", properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError(remoteError);
                return IdentityError("There has been an issue logging you in with the xtremeidiots provider");
            }

            var info = await _xtremeIdiotsAuth.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(HomeController.Index), "Home");

            var username = info.Principal.FindFirstValue(ClaimTypes.Name);

            _logger.LogInformation("User {Username} has successfully authenticated", username);

            var result = await _xtremeIdiotsAuth.ProcessExternalLogin(info);

            switch (result)
            {
                case XtremeIdiotsAuthResult.Success:
                    return RedirectToLocal(returnUrl);
                case XtremeIdiotsAuthResult.Locked:
                    return IdentityError("Your account is currently locked");
                default:
                    return IdentityError("There has been an issue logging you in");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult IdentityError(string message)
        {
            return View(message);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            _logger.LogInformation("User {User} logged out", User.Identity.Name);

            await _xtremeIdiotsAuth.SignOutAsync();

            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}