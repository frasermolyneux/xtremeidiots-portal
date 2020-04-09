using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XI.Portal.Data.Legacy;
using XI.Portal.Data.Legacy.Models;
using XI.Portal.Web.Constants;
using XI.Portal.Web.Extensions;

namespace XI.Portal.Web.Controllers
{
    [Authorize(Policy = XtremeIdiotsPolicy.Management)]
    public class GameServersController : Controller
    {
        private readonly LegacyPortalContext _legacyContext;
        private readonly ILogger<GameServersController> _logger;

        public GameServersController(LegacyPortalContext legacyContext, ILogger<GameServersController> logger)
        {
            _legacyContext = legacyContext ?? throw new ArgumentNullException(nameof(legacyContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _legacyContext.GameServers.ApplyAuthPolicies(User).OrderBy(server => server.BannerServerListPosition).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.GameServers
                .ApplyAuthPolicies(User)
                .FirstOrDefaultAsync(m => m.ServerId == id);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Title,GameType,Hostname,QueryPort,FtpHostname,FtpUsername,FtpPassword,RconPassword,ShowOnBannerServerList,BannerServerListPosition,HtmlBanner,ShowOnPortalServerList,ShowChatLog")]
            GameServers model)
        {
            if (!User.HasGameTypeClaim(model.GameType)) return Unauthorized();

            if (ModelState.IsValid)
            {
                model.ServerId = Guid.NewGuid();
                model.LiveLastUpdated = DateTime.UtcNow;

                _legacyContext.Add(model);

                await _legacyContext.SaveChangesAsync();

                _logger.LogInformation(EventIds.Management, "User {User} has created a new game server with Id {Id}", User.Username(), model.ServerId);

                TempData["Success"] = "A new Game Server has been successfully created";
                return RedirectToAction(nameof(Index));
            }

            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes());

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.GameServers.ApplyAuthPolicies(User).FirstOrDefaultAsync(server => server.ServerId == id);

            if (model == null) return NotFound();

            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes(), model.GameType);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,
            [Bind(
                "ServerId,Title,GameType,Hostname,QueryPort,FtpHostname,FtpUsername,FtpPassword,RconPassword,ShowOnBannerServerList,BannerServerListPosition,HtmlBanner,ShowOnPortalServerList,ShowChatLog")]
            GameServers model)
        {
            if (id != model.ServerId) return NotFound();

            if (ModelState.IsValid)
                try
                {
                    var storedModel = await _legacyContext.GameServers.ApplyAuthPolicies(User).FirstOrDefaultAsync(server => server.ServerId == id);
                    storedModel.Title = model.Title;
                    storedModel.Hostname = model.Hostname;
                    storedModel.QueryPort = model.QueryPort;
                    storedModel.FtpHostname = model.FtpHostname;
                    storedModel.FtpUsername = model.FtpUsername;
                    storedModel.FtpPassword = model.FtpPassword;
                    storedModel.RconPassword = model.RconPassword;
                    storedModel.ShowOnBannerServerList = model.ShowOnBannerServerList;
                    storedModel.BannerServerListPosition = model.BannerServerListPosition;
                    storedModel.HtmlBanner = model.HtmlBanner;
                    storedModel.ShowOnPortalServerList = model.ShowOnPortalServerList;
                    storedModel.ShowChatLog = model.ShowChatLog;

                    _legacyContext.Update(storedModel);

                    await _legacyContext.SaveChangesAsync();

                    _logger.LogInformation(EventIds.Management, "User {User} has modified a game server with Id {Id}", User.Username(), id);

                    TempData["Success"] = "The Game Server has been successfully updated";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameServersExists(model.ServerId))
                        return NotFound();
                    throw;
                }

            ViewData["GameType"] = new SelectList(User.ClaimedGameTypes(), model.GameType);

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var model = await _legacyContext.GameServers.ApplyAuthPolicies(User)
                .FirstOrDefaultAsync(m => m.ServerId == id);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = XtremeIdiotsPolicy.SeniorAdmin)]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var model = await _legacyContext.GameServers.ApplyAuthPolicies(User).FirstOrDefaultAsync(server => server.ServerId == id);

            _legacyContext.GameServers.Remove(model);
            await _legacyContext.SaveChangesAsync();

            _logger.LogInformation(EventIds.Management, "User {User} has deleted a game server with Id {Id}", User.Username(), id);

            TempData["Success"] = "The Game Server has been successfully deleted";
            return RedirectToAction(nameof(Index));
        }

        private bool GameServersExists(Guid id)
        {
            return _legacyContext.GameServers.ApplyAuthPolicies(User).Any(e => e.ServerId == id);
        }
    }
}