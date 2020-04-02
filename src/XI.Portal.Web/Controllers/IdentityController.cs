using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XI.Portal.Web.Controllers
{
    public class IdentityController : Controller
    {
        private readonly Microsoft.AspNetCore.Identity.SignInManager<IdentityUser> _signInManager;

        public IdentityController(Microsoft.AspNetCore.Identity.SignInManager<IdentityUser> signInManager)
        {
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
                //ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                //return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                //return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (result.Succeeded)
            {
                //_logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                //return RedirectToLocal(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                //return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }

            if (result.IsLockedOut)
            {
                //return View("Lockout");
            }
            else
            {
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                var user = new IdentityUser {UserName = name, Email = email};
                var createUserResult = await _signInManager.UserManager.CreateAsync(user);
                if (createUserResult.Succeeded)
                {
                    var addLoginResult = await _signInManager.UserManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        //_logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        if (returnUrl != null)
                            return LocalRedirect(returnUrl);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();

            if (returnUrl != null)
                return LocalRedirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}