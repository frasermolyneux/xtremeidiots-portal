using System;
using System.Linq;
using System.Threading.Tasks;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Data.Legacy;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Data;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
    public class UserController : Controller
    {
        private readonly ApplicationAuthDbContext _authContext;
        private readonly LegacyPortalContext _legacyContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<IdentityUser> _userManager;


        public UserController(
            ApplicationAuthDbContext authContext,
            LegacyPortalContext legacyContext, Microsoft.AspNetCore.Identity.UserManager<IdentityUser> userManager)
        {
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public IActionResult Index()
        {
            var query = from user in _authContext.UserTable.CreateQuery<IdentityUser>()
                where user.Email != ""
                select user;
            var users = query.ToList();

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> LegacyIndex()
        {
            var users = await _legacyContext.AspNetUsers.ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogUserOut(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToAction(nameof(Index));

            var user = await _userManager.FindByIdAsync(id);
            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}