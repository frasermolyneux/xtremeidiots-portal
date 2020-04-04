using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace XI.Portal.Web.Controllers
{
    public class IdentityController : Controller
    {
        private readonly ILogger<IdentityController> _logger;
        private readonly Microsoft.AspNetCore.Identity.SignInManager<IdentityUser> _signInManager;

        public IdentityController(ILogger<IdentityController> logger, Microsoft.AspNetCore.Identity.SignInManager<IdentityUser> signInManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult LoginWithXtremeIdiots(string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Identity", new {ReturnUrl = returnUrl});
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("XtremeIdiots", redirectUrl);
            return new ChallengeResult("XtremeIdiots", properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogError(remoteError);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var username = info.Principal.FindFirstValue(ClaimTypes.Name);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            _logger.LogInformation("User {Username} has successfully authenticated", username);

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            var signInResult = result.ToString();

            _logger.LogInformation("User {Username} had a {SignInResult} sign in result", username, signInResult);

            switch (signInResult)
            {
                case "Lockedout":
                    return IdentityError("Your account is currently locked");
                case "NotAllowed":
                    return IdentityError("There is an issue with your account");
                case "RequiresTwoFactor":
                    return IdentityError("This is currently not implemented");
                case "Succeeded":
                    return RedirectToLocal(returnUrl);
                case "Failed":
                    var user = new IdentityUser { UserName = username, Email = email };
                    var createUserResult = await _signInManager.UserManager.CreateAsync(user);
                    if (createUserResult.Succeeded)
                    {
                        var addLoginResult = await _signInManager.UserManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, false);
                            _logger.LogInformation("User {Username} created a new account with {Email} email", username, email);
                            return RedirectToLocal(returnUrl);
                        }
                    }

                    break;
                default:
                    return RedirectToLocal(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult IdentityError(string message)
        {
            return View(message);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            _logger.LogInformation("User {User} logged out", User.Identity.Name);

            await _signInManager.SignOutAsync();

            if (returnUrl != null)
                return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}