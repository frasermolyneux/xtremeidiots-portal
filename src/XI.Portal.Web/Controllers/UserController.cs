using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Data;
using IdentityUser = ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityUser;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
    public class UserController : Controller
    {
        private readonly ApplicationAuthDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public UserController(ApplicationAuthDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public IActionResult Index()
        {
            var query = from user in _context.UserTable.CreateQuery<IdentityUser>()
                where user.Email != ""
                select user;
            var users = query.ToList();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogUserOut(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            await _userManager.UpdateSecurityStampAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}