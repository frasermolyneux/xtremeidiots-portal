using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XI.CommonTypes;
using XI.Portal.Data.Auth;
using XI.Portal.Data.Legacy;
using XI.Portal.Demos.Models;
using XI.Portal.Demos.Repository;
using IdentityUser = ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityUser;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
    public class MigrationController : Controller
    {
        private readonly IDemoAuthRepository _demoAuthRepository;
        private readonly IDemosRepository _demosRepository;
        private readonly LegacyPortalContext _legacyContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<IdentityUser> _userManager;

        public MigrationController(LegacyPortalContext legacyContext, Microsoft.AspNetCore.Identity.UserManager<IdentityUser> userManager,
            IDemoAuthRepository demoAuthRepository,
            IDemosRepository demosRepository)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _demoAuthRepository = demoAuthRepository ?? throw new ArgumentNullException(nameof(demoAuthRepository));
            _demosRepository = demosRepository ?? throw new ArgumentNullException(nameof(demosRepository));
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

        [HttpGet]
        public async Task<IActionResult> MigrateDemoAuthKeys()
        {
            ViewData["TotalEntries"] = await _legacyContext.AspNetUsers.CountAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProcessMigrateDemoAuthKeys(int progress, int take)
        {
            var log = new StringBuilder();
            var users = await _legacyContext.AspNetUsers.Skip(progress).Take(take).ToListAsync();
            log.AppendLine($"{users.Count} records retrieved from the database, progress {progress}, take {take}");

            foreach (var legacyUser in users)
            {
                log.AppendLine($"Processing demo auth key for {legacyUser.UserName}");
                try
                {
                    var demoAuthKey = await _demoAuthRepository.GetAuthKey(legacyUser.XtremeIdiotsId);

                    if (demoAuthKey == null)
                    {
                        await _demoAuthRepository.UpdateAuthKey(legacyUser.XtremeIdiotsId, legacyUser.DemoManagerAuthKey);
                        log.AppendLine("   Created new demo auth key for user with legacy key");
                    }
                    else
                    {
                        log.AppendLine("   Demo auth key already exists for user");
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

        [HttpGet]
        public async Task<IActionResult> MigrateDemos()
        {
            ViewData["TotalEntries"] = await _legacyContext.Demoes.CountAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProcessMigrateDemos(int progress, int take)
        {
            var log = new StringBuilder();
            var demos = await _legacyContext.Demoes.Include(demo => demo.User).Skip(progress).Take(take).ToListAsync();
            log.AppendLine($"{demos.Count} records retrieved from the database, progress {progress}, take {take}");

            foreach (var demo in demos)
            {
                log.AppendLine($"Processing demo {demo.Name} for {demo.User.UserName}");
                try
                {
                    var existingDemo = await _demosRepository.GetUserDemo(demo.User.XtremeIdiotsId, demo.DemoId.ToString());

                    if (existingDemo == null)
                    {
                        await _demosRepository.UpdateDemo(new DemoDto
                        {
                            RowKey = demo.DemoId.ToString(),
                            UserId = demo.User.XtremeIdiotsId,
                            Game = (GameType) demo.Game,
                            Name = demo.Name,
                            Created = demo.Date,
                            Map = demo.Map,
                            Mod = demo.Mod,
                            Server = demo.Server,
                            Size = demo.Size
                        });

                        log.AppendLine("   Created new demo");
                    }
                    else
                    {
                        log.AppendLine("   Demo already exists - doing nothing");
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