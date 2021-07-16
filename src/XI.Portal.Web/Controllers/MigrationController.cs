using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XI.Portal.Auth.Contract.Constants;
using XI.Portal.Auth.Contract.Models;
using XI.Portal.Bus.Client;
using XI.Portal.Bus.Models;
using XI.Portal.Data.Legacy;
using XI.Portal.Demos.Interfaces;
using XI.Portal.Maps.Interfaces;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = AuthPolicies.AccessMigration)]
    public class MigrationController : Controller
    {
        private readonly IDemoAuthRepository _demoAuthRepository;
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILegacyMapPopularityRepository _mapPopularityRepository;
        private readonly IPortalServiceBusClient _portalServiceBusClient;
        private readonly UserManager<PortalIdentityUser> _userManager;

        public MigrationController(
            LegacyPortalContext legacyContext,
            UserManager<PortalIdentityUser> userManager,
            IDemoAuthRepository demoAuthRepository,
            ILegacyMapPopularityRepository mapPopularityRepository,
            IPortalServiceBusClient portalServiceBusClient)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _demoAuthRepository = demoAuthRepository ?? throw new ArgumentNullException(nameof(demoAuthRepository));
            _mapPopularityRepository = mapPopularityRepository;
            _portalServiceBusClient = portalServiceBusClient;
        }

        [HttpGet]
        public async Task<IActionResult> ExportMapVotes()
        {
            var maps = _legacyContext.Maps;

            foreach (var map in maps)
            {
                var otherVotes = await _mapPopularityRepository.GetMapPopularity(map.GameType, map.MapName);

                if (otherVotes?.MapVotes == null)
                    continue;

                foreach (var vote in otherVotes.MapVotes)
                    await _portalServiceBusClient.PostMapVote(new MapVote
                    {
                        GameType = map.GameType,
                        Guid = _legacyContext.Player2.Single(p => p.PlayerId == vote.PlayerId).Guid,
                        MapName = map.MapName,
                        Like = vote.Like
                    });
            }

            return RedirectToAction("Index", "Home");
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

                        var identityUser = new PortalIdentityUser {Id = legacyUser.XtremeIdiotsId, UserName = legacyUser.UserName, Email = legacyUser.Email};
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
    }
}