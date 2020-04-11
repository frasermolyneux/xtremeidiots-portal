using System;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Data.Auth;
using XI.Portal.Data.Auth.Extensions;
using XI.Portal.Users.Repository;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly Microsoft.AspNetCore.Identity.UserManager<IdentityUser> _userManager;
        private readonly IUsersRepository _usersRepository;


        public UserController(
            IUsersRepository usersRepository, Microsoft.AspNetCore.Identity.UserManager<IdentityUser> userManager,
            ILogger<UserController> logger)
        {
            _usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAjax()
        {
            var users = await _usersRepository.GetUsers();
            return Json(new
            {
                data = users
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogUserOut(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(id);
            await _userManager.UpdateSecurityStampAsync(user);

            TempData["Success"] = $"User {user.UserName} has been force logged out (this may take up to 15 minutes)";
            _logger.LogInformation(EventIds.Management, "User {User} have force logged out {TargetUser}", User.Username(), user.UserName);

            return RedirectToAction(nameof(Index));
        }
    }
}