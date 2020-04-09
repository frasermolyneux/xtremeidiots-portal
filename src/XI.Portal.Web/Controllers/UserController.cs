using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Data;
using XI.Portal.Web.Extensions;
using IdentityUser = ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityUser;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
    public class UserController : Controller
    {
        private readonly ApplicationAuthDbContext _authContext;
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<UserController> _logger;
        private readonly Microsoft.AspNetCore.Identity.UserManager<IdentityUser> _userManager;


        public UserController(
            ApplicationAuthDbContext authContext,
            LegacyPortalContext legacyContext, Microsoft.AspNetCore.Identity.UserManager<IdentityUser> userManager,
            ILogger<UserController> logger)
        {
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            _logger.LogInformation(EventIds.Management, "User {User} have force logged out {TargetUser}", User.Username(), user.UserName);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> MigrateUsers()
        {
            ViewData["TotalEntries"] = await _legacyContext.AspNetUsers.CountAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProcessMigrateUsers(int progress, int take)
        {
            var log = new StringBuilder();
            var users = await _legacyContext.AspNetUsers.Skip(progress).Take(take).ToListAsync();
            log.AppendLine($"{users.Count} records retrieved from the database, progress {progress}, take {take}");

            foreach (var legacyUser in users)
            {
                log.AppendLine($"Processing legacy user {legacyUser.UserName} with email {legacyUser.Email}");
                try
                {
                    var existingIdentityUser = await _userManager.FindByEmailAsync(legacyUser.Email);

                    if (existingIdentityUser == null)
                    {
                        log.AppendLine("   Legacy user has not been migrated");

                        var identityUser = new IdentityUser {Id = legacyUser.XtremeIdiotsId, UserName = legacyUser.UserName, Email = legacyUser.Email};
                        var createUserResult = await _userManager.CreateAsync(identityUser);

                        if (createUserResult.Succeeded)
                        {
                            await _userManager.AddLoginAsync(identityUser, new UserLoginInfo("XtremeIdiots", legacyUser.XtremeIdiotsId, "OAuth"));
                            log.AppendLine("   User has been created with a login");
                        }
                    }
                    else
                    {
                        log.AppendLine("   No action needed - user already exists");
                    }
                }
                catch (Exception ex)
                {
                    log.AppendLine($"   {ex.Message}");
                }
            }

            return Json(new
            {
                progress = progress + take,
                log = log.ToString()
            });
        }
    }
}